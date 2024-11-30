
function updateSlide(slideId, content) {
    connection.invoke("BroadcastSlideUpdate", slideId, content).catch(err => console.error(err));
}

// Establish SignalR connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/collaborationHub")
    .build();

// Handle updated user list
connection.on("UserListUpdated", function (userList) {
    const $userListElement = $("#userList");
    $userListElement.empty();

    $.each(userList, function (index, user) {
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

// Handle slide content updates
$(".slide").on("input", function () {
    const slideId = $(this).attr("id").split("-")[1];
    const slideContent = $(this).text();

    connection.invoke("UpdateSlide", slideId, slideContent).catch(function (err) {
        console.error(err.toString());
    });
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






//function renderMarkdown(text) {
//    const html = marked(text);
//    document.getElementById('markdownPreview').innerHTML = html;
//}

function updateTextBlockMarkdown() {
    var activeObject = canvas.getActiveObject();
    if (activeObject && activeObject.type === 'textbox') {
        var markdownText = activeObject.text;
        //renderMarkdown(markdownText);
    }
}

function deleteSelectedItem() {
    var activeObject = canvas.getActiveObject();
    if (activeObject) {
        canvas.remove(activeObject);
    } else {
        alert("Please select an item to delete.");
    }
}


function saveSlide() {
    var presentationId = $('#presentation-id').val();
    var slideData = JSON.stringify(canvas.toJSON());

    console.log(presentationId);
    console.log(slideData);

    // Need to send the list of slides or the slide model

    $.post('/Presentations/SaveSlide', { presentationId: presentationId, slideData: slideData }, function (response) {
        if (response.success) {
            alert("Slide saved successfully!");
        } else {
            alert("Error saving the slide.");
        }
    });
}


//function loadSlide(slideId) {
//    $.get(`/Presentations/GetSlide?id=${slideId}`, function (response) {
//        if (response.success) {
//            canvas.loadFromJSON(response.slideData, canvas.renderAll.bind(canvas));
//        } else {
//            alert("Error loading the slide.");
//        }
//    });
//}


//function animateObject() {
//    var activeObject = canvas.getActiveObject();
//    if (activeObject) {
//        activeObject.animate('left', '+=50', {
//            onChange: canvas.renderAll.bind(canvas),
//            duration: 1000
//        });
//    }
//}






function exportToPDF() {
    const { jsPDF } = window.jspdf;
    const pdf = new jsPDF();
    var canvasDataUrl = canvas.toDataURL();
    pdf.addImage(canvasDataUrl, 'PNG', 10, 10, 180, 160);
    pdf.save('presentation.pdf');
}