// Shared JavaScript for 1x2, 1x3, 1x4 customization pages

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
    renderCanvas();
}

// Filter selection
function setFilter(id) {
    selectedFilterId = id;
    console.log("Selected filter:", id);
    renderCanvas();
}

// Sticker selection
function setBackSticker(id) {
    selectedStickerBackId = id;
    renderCanvas();
}
function setFrontSticker(id) {
    selectedStickerFrontId = id;
    renderCanvas();
}

// Filter styles -------------------- (No Grains and Vignette yet) -------------------- to be updated after
function getCanvasFilterById(id) {
    switch (id) {
        case 1: return "contrast(130%) saturate(150%) brightness(110%) hue-rotate(15deg)";
        case 2: return "sepia(100%) saturate(85%) contrast(90%) brightness(105%) hue-rotate(15deg)";
        case 3: return "hue-rotate(25deg) saturate(180%) brightness(105%) contrast(120%)";
        case 4: return "brightness(100%) contrast(90%) saturate(124%) hue-rotate(10deg)";
        case 5: return "grayscale(30%) brightness(80%) contrast(115%)";
        default: return "none";
    }
}

// Timestamp toggle
const timestampToggle = document.getElementById("timestampToggle");
if (timestampToggle) {
    timestampToggle.addEventListener("change", function () {
        includeTimestamp = this.checked;
        renderCanvas();
    });
}

// Canvas rendering
function renderCanvas() {
    if (photos.length !== layoutType) {
        console.warn(`Expected ${layoutType} photos. Found:`, photos.length);
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        return;
    }

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

    function finalizeDownload() {
        document.getElementById("download").href = canvas.toDataURL("image/png");
    }
}

// Initial draw
renderCanvas();
