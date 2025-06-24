const video = document.getElementById("video");
const countdownEl = document.querySelector(".countdown");
const cameraFrame = document.querySelector(".camera-frame");
const photoStrip = document.querySelector(".photo-strip");
const startBtn = document.querySelector(".btn-start");
let retakeBtn, doneBtn;

let photoCount = document.querySelectorAll(".strip-frame").length;
let photosTaken = 0;
let imageData = [];

function setupButtons() {
    // Create Retake and Done buttons
    retakeBtn = document.createElement("button");
    retakeBtn.textContent = "Retake";
    retakeBtn.className = "btn-1";
    retakeBtn.style.display = "none";
    retakeBtn.addEventListener("click", resetSession);

    doneBtn = document.createElement("button");
    doneBtn.textContent = "Done";
    doneBtn.className = "btn-1";
    doneBtn.style.display = "none";
    doneBtn.addEventListener("click", () => {
        localStorage.setItem("photos", JSON.stringify(imageData));
        alert("Photos saved in local storage!");
    });

    document.querySelector(".btn-row").appendChild(retakeBtn);
    document.querySelector(".btn-row").appendChild(doneBtn);
}

function resetSession() {
    photosTaken = 0;
    imageData = [];
    document.querySelectorAll(".strip-frame").forEach(f => f.style.backgroundImage = "");
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
    ctx.scale(-1, 1); // Mirror image
    ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

    const photo = canvas.toDataURL("image/jpeg");
    imageData.push(photo);

    const frame = document.querySelectorAll(".strip-frame")[photosTaken];
    frame.style.backgroundImage = `url(${photo})`;

    photosTaken++;
    setTimeout(takeNextPhoto, 500); // Delay before next
}

function startCountdown() {
    return new Promise(resolve => {
        let count = 3;
        countdownEl.textContent = count;

        const countdown = setInterval(() => {
            count--;
            countdownEl.textContent = count > 0 ? count : "📸";

            if (count === 0) {
                clearInterval(countdown);
                setTimeout(() => {
                    countdownEl.textContent = "";
                    resolve();
                }, 500);
            }
        }, 1000);
    });
}

// Request camera access
navigator.mediaDevices.getUserMedia({ video: true })
    .then(stream => {
        video.srcObject = stream;
    })
    .catch(err => {
        alert("Camera access denied or not available.");
    });

// Initialize
startBtn.addEventListener("click", startPhotoSession);
setupButtons();
