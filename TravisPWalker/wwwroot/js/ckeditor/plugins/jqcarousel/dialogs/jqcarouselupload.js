CKEDITOR.dialog.add('jqcarouselupload', function (editor) {
    function clearSlideUploadIdentifier() {
        var slideUpload = $(CKEDITOR.instances.e1.window.getFrame().$).contents().find('.ImageUpdate');
        for (let i = 0; i < slideUpload.length; i++) {
            carouselId = slideUpload[i].setAttribute('carouselid', "");
            slideIndex = slideUpload[i].setAttribute('slideindex', "");
        }
    }

    var carouselId = "";
    var slideIndex = "";
    var newUrl = "";


    return {
        title: 'Upload Image',
        minWidth: 200,
        minHeight: 100,
        init: function () {
            debugger
        },
        onShow: function () {
            //debugger;
            // Detect if we are on a Carousel
            var command = this.getName();
            var slideUpload = $(CKEDITOR.instances.e1.window.getFrame().$).contents().find('.ImageUpdate');            
            for (let i = 0; i < slideUpload.length; i++) {
                if (slideUpload[i].getAttribute('carouselid') != "") {
                    carouselId = slideUpload[i].getAttribute('carouselid');
                    slideIndex = slideUpload[i].getAttribute('slideindex');
                }
            }
            debugger
            if (command == 'jqcarouselupload') {

            }
        },
        contents: [
            {
                id: 'Upload',
                hidden: true,
                filebrowser: 'uploadButton',
                label: 'Upload',
                elements: [{
                    type: 'file',
                    id: 'upload',
                    label: 'Upload',
                    style: 'height:40px',
                    size: 38
                    },
                    {
                    type: 'fileButton',
                    id: 'uploadButton',
                    filebrowser: 'Upload:fileUrl',
                    label: 'Upload',
                    'for': ['Upload', 'upload']
                    },
                    {
                        type: 'text',
                        id: 'fileUrl',
                        onChange: function () {
                            debugger
                            newUrl = this.getValue();     
                            if (newUrl != "") {
                                var slides = $(CKEDITOR.instances.e1.window.getFrame().$).contents().find('.carousel-item.' + carouselId);
                                for (let i = 0; i < slides.length; i++) {
                                    if (slides[i].getAttribute('data-bs-slide-index') == slideIndex.toString()) {
                                        slides[i].children[1].setAttribute('src', newUrl)
                                    }
                                }
                            }
                        }, 
                    },
                ]
            }
        ],
        onOk: function () {
            clearSlideUploadIdentifier();
        },
        onHide: function () {
            clearSlideUploadIdentifier();
        },
        onCancel: function () {
            clearSlideUploadIdentifier();
        }
    };

    
});
