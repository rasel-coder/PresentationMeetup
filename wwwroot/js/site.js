
let slides = [];
let currentSlideIndex = 0;
let canvas = new fabric.Canvas('drawingCanvas');

// Establish SignalR connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/collaborationHub")
    .build();

function adminJoinPresentation(presentationId, nickName, role) {
    console.log("Navigating to presentation details page...");
    window.location.href = `/Presentations/Details?PresentationId=${presentationId}&NickName=${nickName}&role=${role}`;
}

// Handle updated user list
connection.on("UserListUpdated", function (usersInGroup) {
    const $userListElement = $("#userList");
    $userListElement.empty();

    $.each(usersInGroup, function (index, user) {
        const $listItem = $(`
        <li class="d-flex justify-content-between">
            ${user.nickname} (${user.role})
            <div class="dropdown">
                <a class="" data-bs-toggle="dropdown" aria-expanded="false">
                    <img src="/images/three-dot.png" width="18" />
                </a>
                <ul class="dropdown-menu">
                    <li><a class="dropdown-item viewer-option" href="#" data-user-id="${user.Id}">Viewer</a></li>
                    <li><a class="dropdown-item editor-option" href="#" data-user-id="${user.Id}">Editor</a></li>
                </ul>
            </div>
        </li>
    `);
        $userListElement.append($listItem);
    });
});

connection.on("UserDisconnected", function (nickname) {
    console.log(nickname + " has left the presentation.");
});

connection.on("ReceiveSlideUpdate", function (slideId, content) {
    slides[currentSlideIndex].slideId = slideId;
    const updatedIndex = slides.findIndex(slide => slide.slideId === slideId);

    if (updatedIndex !== -1) {
        slides[updatedIndex] = {
            slideId: slideId,
            slideData: content,
        };
        console.log(`Slide ${updatedIndex + 1} updated:`, slides[updatedIndex]);

        if (currentSlideIndex === updatedIndex) {
            let index = currentSlideIndex;
            if (index >= 0 && index < slides.length) {
                const slide = slides[index];
                $('#slide-page-count').html(`${index + 1} of ${slides.length}`);

                if (slide && slide.slideData) {
                    canvas.loadFromJSON(slide.slideData, function () {
                        canvas.renderAll();
                        console.log(`Slide ${index + 1} rendered.`);
                    });
                } else {
                    canvas.clear();
                    console.warn(`Slide ${index + 1} has no data to render.`);
                }
            } else {
                console.error("Invalid slide index:", index);
            }
        }
    } else {
        console.warn("Slide with the specified ID not found:", slideId);
    }
    initializeSlide(currentSlideIndex);
});

connection.on("ReceiveSlideDelete", function () {
    slides.splice(currentSlideIndex, 1);

    if (currentSlideIndex >= slides.length) {
        currentSlideIndex = slides.length - 1;
    }

    if (slides.length > 0) {
        console.log(currentSlideIndex);
        initializeSlide(currentSlideIndex);
    } else {
        canvas.clear();
        alert('No slides left in the presentation.');
    }
});

//$('.slide-status').on('click', function () {
//    initializeSlide(currentSlideIndex);
//});



canvas.on("object:modified", function () {
    updateSlide();
});

// Optionally handle manual user disconnection
$(window).on("beforeunload", function () {
    connection.stop().catch(function (err) {
        console.error(err.toString());
    });
});






$('#join-title').on('show.bs.modal', function (event) {
    const user = $(event.relatedTarget);
    const presentationId = user.data('presentation-id');
    const presentationTitle = user.data('presentation-title');

    $('#presentationId').val(presentationId);
    $('#presentationTitle').html(presentationTitle);
});





function updateSlide() {
    const slideId = slides[currentSlideIndex].slideId;
    const slideData = JSON.stringify(canvas.toJSON());

    connection.invoke("UpdateSlide", slideId, slideData).catch(function (err) {
        console.error(err.toString());
    });
}



function deleteSelectedItem() {
    var activeObject = canvas.getActiveObject();
    if (activeObject) {
        canvas.remove(activeObject);
    } else {
        alert("Please select an item to delete.");
    }
    updateSlide();
}

function loadSlides(initialSlides) {
    slides = initialSlides.map((slide) => ({
        slideId: slide.slideId,
        slideData: slide.slideData ? JSON.parse(slide.slideData) : null,
    }));

    if (slides.length > 0) {
        currentSlideIndex = 0;
        initializeSlide(currentSlideIndex);
    }
    else {
        slides.push({
            slideId: slides.length,
            slideData: {},
        });
    }
}

function initializeSlide(index) {
    const slide = slides[index];
    $('#slide-page-count').html(`${index + 1} of ${slides.length}`);
    if (slides[index]) {
        canvas.loadFromJSON(slide.slideData, canvas.renderAll.bind(canvas));
    } else {
        canvas.clear();
    }
}

function saveCurrentSlide() {
    if (currentSlideIndex >= 0 && currentSlideIndex < slides.length) {
        slides[currentSlideIndex] = {
            slideId: slides[currentSlideIndex].slideId,
            slideData: JSON.stringify(canvas.toJSON()),
        };
    } else {
        console.warn("Invalid currentSlideIndex:", currentSlideIndex);
    }
}

function addNewSlide() {
    saveCurrentSlide();
    slides.push({
        slideId: 0,
        slideData: {},
    });

    currentSlideIndex = slides.length - 1;
    const slideId = 0;
    const slideData = JSON.stringify({});

    connection.invoke("UpdateSlide", slideId, slideData)
        .then(function (updatedSlideId) {
            slides[currentSlideIndex].slideId = updatedSlideId;
            console.log(`New slide created with ID: ${updatedSlideId}`);
        })
        .catch(function (err) {
            console.error("Error creating/updating slide:", err.toString());
        });
    initializeSlide(currentSlideIndex);
}

function nextSlide() {
    if (currentSlideIndex < slides.length - 1) {
        saveCurrentSlide();
        currentSlideIndex++;
        initializeSlide(currentSlideIndex);
        updateSlide();
    } else {
        alert('This is the last slide.');
    }
}

function prevSlide() {
    if (currentSlideIndex > 0) {
        saveCurrentSlide();
        currentSlideIndex--;
        initializeSlide(currentSlideIndex);
        updateSlide();
    } else {
        alert('This is the first slide.');
    }
}

function saveSlides() {
    saveCurrentSlide();
    const presentationId = $('#presentation-id').val();

    const formData = new FormData();
    formData.append('presentationId', presentationId);

    slides.forEach((slide, index) => {
        formData.append(`slides[${index}].slideId`, slide.slideId);
        formData.append(`slides[${index}].slideData`, slide.slideData || '');
    });

    $.ajax({
        url: '/Presentations/SaveSlides',
        type: 'POST',
        processData: false,
        contentType: false,
        data: formData,
        success: function (response) {
            alert('All slides saved successfully');
        },
        error: function () {
            alert('Error saving slides');
        }
    });
}

function clearCanvas() {
    if (!confirm(`Are you sure you want to delete canvas ${currentSlideIndex + 1}?`)) {
        return;
    }

    var slideId = slides[currentSlideIndex].slideId;
    console.log(slideId);
    connection.invoke("DeleteSlide", slideId).catch(function (err) {
        console.error(err.toString());
    });

    slides.splice(currentSlideIndex, 1);

    if (currentSlideIndex >= slides.length) {
        currentSlideIndex = slides.length - 1;
    }

    if (slides.length > 0) {
        initializeSlide(currentSlideIndex);
        updateSlide();
    } else {
        canvas.clear();
        alert('No slides left in the presentation.');
    }
}


$('#newSlideBtn').on('click', addNewSlide);
$('#nextSlideBtn').on('click', nextSlide);
$('#prevSlideBtn').on('click', prevSlide);
$('#saveSlidesBtn').on('click', saveSlides);

initializeSlide(currentSlideIndex);

function exportToPDF() {
    const { jsPDF } = window.jspdf;
    const pdf = new jsPDF();
    var canvasDataUrl = canvas.toDataURL();
    pdf.addImage(canvasDataUrl, 'PNG', 10, 10, 180, 160);
    pdf.save('presentation.pdf');
}