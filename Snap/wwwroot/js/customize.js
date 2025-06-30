// Shared JavaScript for 1x2, 1x3, 1x4 customization pages
const layoutType = parseInt(localStorage.getItem("layoutType"), 10);
//const layoutFolder = layoutType === 2 ? "2" : layoutType === 3 ? "3" : "4";
const canvas = document.getElementById("strip-fr");
const ctx = canvas.getContext("2d");

document.fonts.load("20px 'TR Candice'").then(() => {
    renderCanvas();
});

// Retrieve photos from localStorage
const photos = JSON.parse(localStorage.getItem("photos") || "[]");

let selectedFrameColor = "#BA5E62";
let selectedFilterId = 0;
let selectedStickerBackId = 0;
let selectedStickerFrontId = 0;
let includeTimestamp = false;

// Frame color selection
function setFrameColor(color) {
    selectedFrameColor = color;
    localStorage.setItem("frameColor", color);
    renderCanvas();
}

// Filter selection
function setFilter(id) {
    selectedFilterId = id;
    localStorage.setItem("filterId", id);
    renderCanvas();
}

// Sticker selection
function setBackSticker(id) {
    selectedStickerBackId = id;
    localStorage.setItem("stickerId", id);
    renderCanvas();
}
function setFrontSticker(id) {
    selectedStickerFrontId = id;
    localStorage.setItem("stickerId", id);
    renderCanvas();
}

// Filter styles
function getCanvasFilterById(id) {
    switch (id) {
        case 1: return "contrast(130%) saturate(150%) brightness(110%) hue-rotate(15deg)"; // Retro Pop
        case 2: return "sepia(100%) saturate(85%) contrast(90%) brightness(105%) hue-rotate(15deg)"; // Vintage Film
        case 3: return "hue-rotate(25deg) saturate(180%) contrast(120%) brightness(105%)"; // Retro Sunset
        case 4: return "brightness(100%) contrast(90%) saturate(124%) hue-rotate(-10deg)"; // Polaroid Hush
        case 5: return "saturate(70%) contrast(150%) brightness(95%)"; // Moody Vinyl
        default: return "none";
    }
}

// Solid overlay
function applySolidOverlayToArea(ctx, x, y, width, height, rgbaColor) {
    ctx.fillStyle = rgbaColor;
    ctx.fillRect(x, y, width, height);
}

// Gradient overlay for individual photo
function applyGradientOverlayToArea(ctx, x, y, width, height, colorStops) {
    const gradient = ctx.createLinearGradient(x, y, x, y + height);
    colorStops.forEach(stop => gradient.addColorStop(stop.offset, stop.color));
    ctx.fillStyle = gradient;
    ctx.fillRect(x, y, width, height);
}

// Vignette overlay for individual photo
function applyVignetteOverlayToArea(ctx, x, y, width, height, opacity = 0.4) {
    const centerX = x + width / 2;
    const centerY = y + height / 2;
    const gradient = ctx.createRadialGradient(centerX, centerY, 0, centerX, centerY, width * 0.75);
    gradient.addColorStop(0, 'rgba(0, 0, 0, 0)');
    gradient.addColorStop(1, `rgba(0, 0, 0, ${opacity})`);
    ctx.fillStyle = gradient;
    ctx.fillRect(x, y, width, height);
}

// Add grain Noise effect
function addGrainNoiseToArea(ctx, x, y, width, height, intensity = 0.05) {
    const imageData = ctx.getImageData(x, y, width, height);
    const data = imageData.data;

    for (let i = 0; i < data.length; i += 4) {
        const noise = (Math.random() * 2 - 1) * 255 * intensity;

        data[i] = Math.min(255, Math.max(0, data[i] + noise));     // R
        data[i + 1] = Math.min(255, Math.max(0, data[i + 1] + noise)); // G
        data[i + 2] = Math.min(255, Math.max(0, data[i + 2] + noise)); // B
    }

    ctx.putImageData(imageData, x, y);
}

// Timestamp toggle
const timestampToggle = document.getElementById("timestampToggle");
if (timestampToggle) {
    timestampToggle.addEventListener("change", function () {
        includeTimestamp = this.checked;
        localStorage.setItem("includeTimestamp", includeTimestamp.toString());
        renderCanvas();
    });
}

// Canvas rendering
function renderCanvas() {
    //if (photos.length !== layoutType) {
    //    console.warn(`Expected ${layoutType} photos. Found:`, photos.length);
    //    ctx.clearRect(0, 0, canvas.width, canvas.height);
    //    return;
    //}
    console.log("layoutType from localStorage:", layoutType);
    console.log("Photos loaded from localStorage:", photos);

    const photoWidth = 250;
    const photoHeight = 180;
    const sidePadding = 13;
    const topPadding = 15;
    const spacingBetweenPhotos = 13;
    const extraBottom = 56;

    const totalWidth = photoWidth + sidePadding * 2;
    const canvasHeight = topPadding + (photoHeight * layoutType) + (spacingBetweenPhotos * (layoutType - 1)) + extraBottom;

    // Set physical size
    canvas.width = totalWidth;
    canvas.height = canvasHeight;

    // Fill background
    ctx.fillStyle = selectedFrameColor;
    ctx.fillRect(0, 0, totalWidth, canvasHeight);

    // Draw background sticker
    if (selectedStickerBackId !== 0) {
        const backSticker = new Image();
        backSticker.src = `/Assets/sticker${layoutFolder}/sticker_back${selectedStickerBackId}.png`;
        backSticker.onload = () => {
            ctx.drawImage(backSticker, 0, 0, totalWidth, canvasHeight);
            drawPhotos();
        };
    } else {
        drawPhotos();
    }

    function drawPhotos() {
        let loaded = 0;

        photos.forEach((dataUrl, i) => {
            const img = new Image();
            img.src = dataUrl;
            img.onload = () => {
                const offsetY = topPadding + i * (photoHeight + spacingBetweenPhotos);
                ctx.filter = getCanvasFilterById(selectedFilterId);
                ctx.drawImage(img, sidePadding, offsetY, photoWidth, photoHeight);
                ctx.filter = "none";

                // Apply overlay filter effects individually per photo (solid color, gradient color, vignette, and grain noise effect)
                if (selectedFilterId === 1) {
                    applySolidOverlayToArea(ctx, sidePadding, offsetY, photoWidth, photoHeight, 'rgba(255, 204, 153, 0.2)');
                    addGrainNoiseToArea(ctx, sidePadding, offsetY, photoWidth, photoHeight, 0.03);
                } else if (selectedFilterId === 2) {
                    applyVignetteOverlayToArea(ctx, sidePadding, offsetY, photoWidth, photoHeight, 0.4);
                    addGrainNoiseToArea(ctx, sidePadding, offsetY, photoWidth, photoHeight, 0.015);
                } else if (selectedFilterId === 3) {
                    applyGradientOverlayToArea(ctx, sidePadding, offsetY, photoWidth, photoHeight, [
                        { offset: 0, color: 'rgba(255, 183, 76, 0.27)' },
                        { offset: 1, color: 'rgba(255, 94, 151, 0.27)' }
                    ]);
                } else if (selectedFilterId === 4) {
                    applySolidOverlayToArea(ctx, sidePadding, offsetY, photoWidth, photoHeight, 'rgba(180, 255, 200, 0.1)');
                    applyVignetteOverlayToArea(ctx, sidePadding, offsetY, photoWidth, photoHeight, 0.31);
                    addGrainNoiseToArea(ctx, sidePadding, offsetY, photoWidth, photoHeight, 0.03);
                } else if (selectedFilterId === 5) {
                    applyVignetteOverlayToArea(ctx, sidePadding, offsetY, photoWidth, photoHeight, 0.4);
                    addGrainNoiseToArea(ctx, sidePadding, offsetY, photoWidth, photoHeight, 0.02);
                }

                loaded++;
                if (loaded === layoutType) {

                    // Brand name
                    ctx.fillStyle = "#F9E5DA";
                    ctx.font = "300 16px 'TR Candice', cursive";
                    ctx.textAlign = "center";
                    ctx.fillText("Snapstalgia", canvas.width / 2, canvas.height - 30);

                    // Timestamp
                    if (includeTimestamp) {
                        ctx.font = "8px 'Bricolage Grotesque', sans-serif";
                        ctx.fillText(new Date().toLocaleString(), totalWidth / 2, canvasHeight - 15);
                    }

                    drawFrontSticker();
                }
            };
        });
    }

    // Draw front sticker
    function drawFrontSticker() {
        if (selectedStickerFrontId !== 0) {
            const frontSticker = new Image();
            frontSticker.src = `/Assets/sticker${layoutFolder}/sticker_front${selectedStickerFrontId}.png`;
            frontSticker.onload = () => {
                ctx.drawImage(frontSticker, 0, 0, totalWidth, canvasHeight);
                finalizeDownload();
            };
        } else {
            finalizeDownload();
        }
    }

    //download function
    function finalizeDownload() {
        const downloadLink = document.getElementById("download");
        downloadLink.href = canvas.toDataURL("image/png");
    }
}

// Initial draw
renderCanvas();

document.getElementById("uploadBtn").addEventListener("click", async () => {
    try {
        const sessionId = localStorage.getItem("sessionId");
        const layoutType = parseInt(localStorage.getItem("layoutType"), 10);

        //const filterId = parseInt(localStorage.getItem("filterId")) || 0;
        //const stickerId = localStorage.getItem("stickerId") ? parseInt(localStorage.getItem("stickerId")) : null;
        //const frameColor = localStorage.getItem("frameColor") || "";
        //const includeTimestamp = localStorage.getItem("includeTimestamp") === "true";

        const filterId = selectedFilterId;
        const frameColor = selectedFrameColor;
        const includeTimestampFlag = includeTimestamp;
        const stickerId = selectedStickerFrontId;

        console.log("Uploading with payload:", {
            sessionId,
            layoutType,
            filterId,
            stickerId,
            frameColor,
            includeTimestamp,
        });

        if (!sessionId || !layoutType) {
            alert("Missing session or layout information.");
            return;
        }

        // Delay to ensure image files are saved before compile
        await new Promise(resolve => setTimeout(resolve, 750)); // 750ms

        const payload = {
            SessionId: sessionId,
            LayoutType: layoutType,
            FilterId: filterId,
            StickerId: stickerId,
            FrameColor: frameColor,
            IncludeTimestamp: includeTimestamp
        };

        const response = await fetch("https://localhost:7238/api/photo/compile", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });
        console.log("Calling /api/photo/compile...");

        if (!response.ok) {
            const error = await response.text();
            alert("Failed to upload: " + error);
            return;
        }

        const result = await response.json();
        alert("Photo uploaded and saved successfully!");
        
        window.location.href = "/Profile/ProfilePage";
    } catch (err) {
        alert("Unexpected error: " + err.message);
    }
});