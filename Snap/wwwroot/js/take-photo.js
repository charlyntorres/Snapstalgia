//const video = document.getElementById("video");
//const countdownEl = document.querySelector(".countdown");
//const cameraFrame = document.querySelector(".camera-frame");
//const photoStrip = document.querySelector(".photo-strip");
//const startBtn = document.querySelector(".btn-start");
//let retakeBtn, doneBtn;

//let photoCount = document.querySelectorAll(".strip-frame").length;
//let photosTaken = 0;
//let imageData = [];

//function setupButtons() {
//    // Create Retake and Done buttons
//    retakeBtn = document.createElement("button");
//    retakeBtn.textContent = "Retake";
//    retakeBtn.className = "btn-1";
//    retakeBtn.style.display = "none";
//    retakeBtn.addEventListener("click", resetSession);

//    doneBtn = document.createElement("button");
//    doneBtn.textContent = "Done";
//    doneBtn.className = "btn-1";
//    doneBtn.style.display = "none";
//    doneBtn.addEventListener("click", () => {
//        localStorage.setItem("photos", JSON.stringify(imageData));
//        alert("Photos saved in local storage!");
//    });

//    document.querySelector(".btn-row").appendChild(retakeBtn);
//    document.querySelector(".btn-row").appendChild(doneBtn);
//}

//function resetSession() {
//    photosTaken = 0;
//    imageData = [];
//    document.querySelectorAll(".strip-frame").forEach(f => f.style.backgroundImage = "");
//    countdownEl.textContent = "3";
//    startBtn.style.display = "inline-block";
//    retakeBtn.style.display = "none";
//    doneBtn.style.display = "none";
//}

//function startPhotoSession() {
//    photosTaken = 0;
//    imageData = [];
//    startBtn.style.display = "none";
//    takeNextPhoto();
//}

//async function takeNextPhoto() {
//    if (photosTaken >= photoCount) {
//        retakeBtn.style.display = "inline-block";
//        doneBtn.style.display = "inline-block";
//        return;
//    }

//    await startCountdown();

//    const canvas = document.createElement("canvas");
//    canvas.width = video.videoWidth;
//    canvas.height = video.videoHeight;
//    const ctx = canvas.getContext("2d");

//    ctx.translate(canvas.width, 0);
//    ctx.scale(-1, 1); // Mirror image
//    ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

//    const photo = canvas.toDataURL("image/jpeg");
//    imageData.push(photo);

//    const frame = document.querySelectorAll(".strip-frame")[photosTaken];
//    frame.style.backgroundImage = `url(${photo})`;

//    photosTaken++;
//    setTimeout(takeNextPhoto, 500); // Delay before next
//}

//function startCountdown() {
//    return new Promise(resolve => {
//        let count = 3;
//        countdownEl.textContent = count;

//        const countdown = setInterval(() => {
//            count--;
//            countdownEl.textContent = count > 0 ? count : "📸";

//            if (count === 0) {
//                clearInterval(countdown);
//                setTimeout(() => {
//                    countdownEl.textContent = "";
//                    resolve();
//                }, 500);
//            }
//        }, 1000);
//    });
//}

//// Request camera access
//navigator.mediaDevices.getUserMedia({ video: true })
//    .then(stream => {
//        video.srcObject = stream;
//    })
//    .catch(err => {
//        alert("Camera access denied or not available.");
//    });

//// Initialize
//startBtn.addEventListener("click", startPhotoSession);
//setupButtons();


//// redirect to designated customize photo per layout when done
//doneBtn.addEventListener("click", () => {
//    localStorage.setItem("photos", JSON.stringify(imageData));
//    const layout = startBtn.dataset.layout;

//    if (layout === "1x2") {
//        window.location.href = "/ChooseLayout/Customize1x2photo";
//    } else if (layout === "1x4") {
//        window.location.href = "/ChooseLayout/Customize1x4photo";
//    } else {
//        window.location.href = "/ChooseLayout/Customize1x3photo";
//    }
//});

console.log("Initial localStorage layoutType:", localStorage.getItem("layoutType"));

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

function generateSessionId() {
    // Simple unique ID based on timestamp and random number
    return 'sess-' + Date.now() + '-' + Math.floor(Math.random() * 10000);
}

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
    doneBtn.addEventListener("click", uploadAllPhotos);

    btnRow.appendChild(retakeBtn);
    btnRow.appendChild(doneBtn);
}

function resetSession() {
    photosTaken = 0;
    imageData = [];
    photoFrames.forEach(f => f.style.backgroundImage = "");
    countdownEl.textContent = "3";
    startBtn.style.display = "inline-block";
    retakeBtn.style.display = "none";
    doneBtn.style.display = "none";
}

function startPhotoSession() {
    photosTaken = 0;
    imageData = [];
    startBtn.style.display = "none";
    takeNextPhoto();
}

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

function base64ToBlob(base64) {
    const byteString = atob(base64.split(',')[1]);
    const ab = new ArrayBuffer(byteString.length);
    const ia = new Uint8Array(ab);
    for (let i = 0; i < byteString.length; i++) {
        ia[i] = byteString.charCodeAt(i);
    }
    return new Blob([ab], { type: 'image/png' });
}

async function uploadPhoto(base64Data, sessionId, sequence, layoutType) {
    console.log("uploadPhoto called with layoutType:", layoutType);
    const blob = base64ToBlob(base64Data);
    const formData = new FormData();
    formData.append('File', blob, `${sessionId}_${sequence}.png`);
    formData.append('SessionId', sessionId);
    formData.append('Sequence', sequence);
    formData.append('LayoutType', layoutType);

    console.log('Sending form data:');
    for (const pair of formData.entries()) {
        console.log(`${pair[0]}: ${pair[1]}`);
    }

    const response = await fetch('https://localhost:7238/api/photo/upload', {
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
        console.error('Upload failed:', errorText);
        throw new Error(errorText || `Upload failed with status ${response.status}`);
    }


    return response.json();
}

function parseLayoutType(layoutStr) {
    const match = layoutStr.match(/\d+$/);
    return match ? parseInt(match[0], 10) : 0;
}

async function uploadAllPhotos() {
    try {
        const storedLayout = localStorage.getItem("layoutType");
        console.log('Stored layout from localStorage:', storedLayout);
        if (!storedLayout) {
            alert("Layout type not set in localStorage. Please select a layout first.");
            return;
        }
        const layout = parseInt(storedLayout, 10);
        console.log('Parsed layout:', layout);
        // Throw if invalid
        if (isNaN(layout)) throw new Error('Invalid layoutType: ' + storedLayout);

        for (let i = 0; i < imageData.length; i++) {
            await uploadPhoto(imageData[i], sessionId, i, layout);
        }

        localStorage.setItem('sessionId', sessionId);
        alert('All photos uploaded successfully! Redirecting...');

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
