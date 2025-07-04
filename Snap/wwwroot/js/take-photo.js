const video = document.getElementById("video");
const countdownEl = document.querySelector(".countdown");
const startBtn = document.querySelector(".btn-start");
const btnRow = document.querySelector(".btn-row");
let retakeBtn, doneBtn;

const photoFrames = document.querySelectorAll(".strip-frame");
const photoCount = photoFrames.length;

let photosTaken = 0;
let imageData = [];
let sessionId = generateSessionId();

// Generate unique Session ID
function generateSessionId() {
    // Simple unique ID based on timestamp and random number
    return 'session-' + Date.now() + '-' + Math.floor(Math.random() * 10000);
}

// Dynamically create Retake and Done buttons after taking all photos
function setupButtons() {
    // Retake button
    retakeBtn = document.createElement("button");
    retakeBtn.textContent = "Retake";
    retakeBtn.className = "btn-1";
    retakeBtn.style.display = "none";
    retakeBtn.addEventListener("click", resetSession);

    // Done button
    doneBtn = document.createElement("button");
    doneBtn.textContent = "Done";
    doneBtn.className = "btn-1";
    doneBtn.style.display = "none";
    doneBtn.addEventListener("click", () => {
        localStorage.setItem("photos", JSON.stringify(imageData));
    });
    doneBtn.addEventListener("click", uploadAllPhotos);

    btnRow.appendChild(retakeBtn);
    btnRow.appendChild(doneBtn);
}

// Reset session
function resetSession() {
    photosTaken = 0;
    imageData = [];
    localStorage.removeItem("photos");
    photoFrames.forEach(f => f.style.backgroundImage = "");
    countdownEl.textContent = "3";
    startBtn.style.display = "inline-block";
    retakeBtn.style.display = "none";
    doneBtn.style.display = "none";
}

// Start session
function startPhotoSession() {
    resetSession();
    photosTaken = 0;
    imageData = [];
    startBtn.style.display = "none";
    takeNextPhoto();
}

// Take next photo
async function takeNextPhoto() {
    if (photosTaken >= photoCount) {
        retakeBtn.style.display = "inline-block";
        doneBtn.style.display = "inline-block";
        return;
    }

    await startCountdown();

    const canvas = document.createElement("canvas");
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    const ctx = canvas.getContext("2d");

    ctx.translate(canvas.width, 0);
    ctx.scale(-1, 1); // Mirror image horizontally
    ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

    const photo = canvas.toDataURL("image/jpeg");
    imageData.push(photo);

    photoFrames[photosTaken].style.backgroundImage = `url(${photo})`;

    photosTaken++;
    setTimeout(takeNextPhoto, 500);
}

// Start capture countdown
function startCountdown() {
    return new Promise(resolve => {
        let count = 3;
        countdownEl.textContent = count;

        const interval = setInterval(() => {
            count--;
            countdownEl.textContent = count > 0 ? count : "📸";

            if (count === 0) {
                clearInterval(interval);
                setTimeout(() => {
                    countdownEl.textContent = "";
                    resolve();
                }, 500);
            }
        }, 1000);
    });
}

// Convert image to blob file
function base64ToBlob(base64) {
    const byteString = atob(base64.split(',')[1]);
    const ab = new ArrayBuffer(byteString.length);
    const ia = new Uint8Array(ab);
    for (let i = 0; i < byteString.length; i++) {
        ia[i] = byteString.charCodeAt(i);
    }
    return new Blob([ab], { type: 'image/png' });
}

// Upload individual photos of a session to db
async function uploadPhoto(base64Data, sessionId, sequence, layoutType) {
    const blob = base64ToBlob(base64Data);
    const formData = new FormData();

    formData.append('File', blob, `${sessionId}_${sequence}.png`);
    formData.append('SessionId', sessionId);
    formData.append('Sequence', sequence);
    formData.append('LayoutType', layoutType);

    const response = await fetch('/api/photo/upload', {
        method: 'POST',
        body: formData
    });

    if (!response.ok) {
        let errorText;
        try {
            const errorData = await response.json();
            errorText = errorData.message || JSON.stringify(errorData);
        } catch {
            errorText = await response.text();
        }
        throw new Error(errorText || `Upload failed with status ${response.status}`);
    }
    return response.json();
}

// Parse layout type from string to int
function parseLayoutType(layoutStr) {
    const match = layoutStr.match(/\d+$/);
    return match ? parseInt(match[0], 10) : 0;
}

// Upload all photos of a session to db
async function uploadAllPhotos() {
    try {
        const storedLayout = localStorage.getItem("layoutType");
        if (!storedLayout) {
            alert("Layout type not set in localStorage. Please select a layout first.");
            return;
        }

        const layout = parseInt(storedLayout, 10);
        if (isNaN(layout)) throw new Error('Invalid layoutType: ' + storedLayout);

        for (let i = 0; i < imageData.length; i++) {
            await uploadPhoto(imageData[i], sessionId, i, layout);
        }

        localStorage.setItem('sessionId', sessionId);

        // Add delay to ensure files are flushed on backend
        await new Promise(resolve => setTimeout(resolve, 500));

        localStorage.setItem("photos", JSON.stringify(imageData));
        localStorage.setItem("sessionId", sessionId);

        if (layout === 2) {
            window.location.href = "/ChooseLayout/Customize1x2photo";
        } else if (layout === 4) {
            window.location.href = "/ChooseLayout/Customize1x4photo";
        } else {
            window.location.href = "/ChooseLayout/Customize1x3photo";
        }

    } catch (error) {
        alert("Error uploading photos: " + error.message);
    }
}

// Camera access request and initialization
navigator.mediaDevices.getUserMedia({ video: true })
    .then(stream => {
        video.srcObject = stream;
    })
    .catch(() => {
        alert("Camera access denied or not available.");
    });

// Initialize buttons and event listeners
setupButtons();
startBtn.addEventListener("click", startPhotoSession);
