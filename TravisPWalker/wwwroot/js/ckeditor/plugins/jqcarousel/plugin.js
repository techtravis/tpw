(function () {
    CKEDITOR.plugins.add('jqcarousel', {
        requires: 'widget,dialog',
        icons: 'jqcarousel',
        afterInit: function () {
            
        },
        init: function (editor) {   
            var pluginPath = this.path;            
            CKEDITOR.dialog.add('jqcarousel', pluginPath + 'dialogs/jqcarousel.js');    
            CKEDITOR.dialog.add('jqcarouselupload', pluginPath + 'dialogs/jqcarouselupload.js'); 

            CKEDITOR.scriptLoader.load('../lib/jquery-ui/dist/jquery-ui-1.12.1.js');
            CKEDITOR.scriptLoader.load('../lib/jquery-imagedrag/dist/jquery.imagedrag.js');
            CKEDITOR.scriptLoader.load('../lib/jquery-easing/dist/jquery.easing.min.js');
            CKEDITOR.scriptLoader.load('../lib/jquery-fittext/dist/jquery.fittext.js');
            CKEDITOR.scriptLoader.load('../js/wow.min.js');

            CKEDITOR.scriptLoader.load(CKEDITOR.getUrl(pluginPath + 'lib/helper.js'));

            editor.addContentsCss(this.path + 'styles/jqcarousel.css');

            editor.addCommand('jqcarouselRequestUpload',
                CKEDITOR.tools.extend(new CKEDITOR.dialogCommand('jqcarouselupload'), { modes: { wysiwyg: 1, source: 1 }, state: CKEDITOR.TRISTATE_OFF })
            );

            // Add widget
            editor.ui.addButton('jqcarousel', {
                label: 'Create a Carousel',
                command: 'jqcarousel',
                icon: this.path + 'icons/jqcarousel.png'
            });
            editor.widgets.add('jqcarousel',
                {
                    allowedContent: 'div(!jqcarousel);',
                    requiredContent: 'div(!jqcarousel);',
                    parts: {
                        jqcarousel: 'div.jqcarousel',
                    },
                    //editables: {
                    //    content: '',
                    //},
                    template:
                        '<div class="jqcarousel row">'+
                            '<div class="container-fluid">' +
                            '    <div id="carousel-1" class="carousel slide" data-bs-ride="carousel">' +
                            '        <!-- Indicators -->' +
                            '        <ol class="carousel-indicators">' +
                            '            <li data-bs-target="#carousel-1" data-bs-slide-to="0" class="active"></li>' +
                            '            <li data-bs-target="#carousel-1" data-bs-slide-to="1"></li>' +
                            '        </ol>' +
                            '' +
                            '        <!-- Wrapper for slides -->' +
                            '        <div class="carousel-inner" role="listbox">' +
                            '            <div class="carousel-item carousel-1 active" data-bs-slide-index="0">' +

                            '                <img src="../images/space.jpeg" alt="..." class="w100p">' +
                            '                <div class="carousel-caption"> ' +
                            //'                    Slider 1 Caption' +
                            '                </div>' +
                            '            </div>' +
                            '            <div class="carousel-item carousel-1" data-bs-slide-index="1">' +

                            '                <img src="../images/field.jpeg" alt="..." class="w100p">' +
                            '                <div class="carousel-caption"> ' +
                            //'                    Slider 2 Caption' +
                            '                </div>' +
                            '            </div>' +
                            '        </div>' +
                            '        <div class="ImageUpdate" carouselid="" slideindex=""><div>' +
                            '' +
                            '        <!-- Controls -->' +
                            '        <button class="carousel-control carousel-control-prev" type="button" data-bs-target="#carousel-1" data-bs-slide="prev">' +
                            '			<span class="carousel-control-prev-icon" aria-hidden="true"></span>' +
                            '			<span class="visually-hidden">Previous</span>' +
                            '        </button>' +
                            '        <button class="carousel-control carousel-control-next" type="button" data-bs-target="#carousel-1" data-bs-slide="next">' +
                            '			<span class="carousel-control-next-icon" aria-hidden="true"></span>' +
                            '			<span class="visually-hidden">Next</span>' +
                            '        </button>' +
                            '    </div>' +
                            '</div>' +
                            '' +
                            '<style type="text/css">' +
                            '    .carousel-item.carousel-1 {' +
                            '        height: 600px;' +
                            '    }' +
                            '</style>' +                            
                        '</div>',
                    dialog: 'jqcarousel',
                    defaults: {
                    },
                    // Before init.
                    upcast: function (element) {                        
                        if (element.name == 'div' && element.hasClass('jqcarousel')) {      
                            // Executed when CKEditor loads content, when switching from source to wysiwyg mode. Makes HTML content a widget.
                            return true;
                        } else {
                            return false;
                        }
                    },
                    downcast: function (element) {
                        // Executed when CKEditor returns content, when switching from wysiwyg to source mode. Transforms a widget back to a downcasted form.
                        //does not affect html parsed data that is saved unfortunately
                    },
                    // initialize
                    // Init function copy/paste hits this.
                    init: function () {
                        // We need to rename all the components so they are unique to this carousel
                        // unless it already has a unique name
                        this.setData('ok', '');
                        this.setData('carouselHeight', '600px');
                        this.setData('guid', guid());                       
                        var carouselObj = this.element.findOne('#carousel-1');
                        if (carouselObj != null) {
                            var carouselObjId = carouselObj.getAttribute('id');
                            if (carouselObjId == 'carousel-1') {
                                carouselObj.setAttribute('id', this.data.guid);
                            }

                            var carouselLIs = this.element.find('li').toArray();
                            var liCount = carouselLIs.length;
                            for (let i = 0; i < liCount; i++) {
                                carouselLIs[i].setAttribute('data-bs-target', '#' + this.data.guid);
                            }
                            
                            var items = this.element.find('div.carousel-item').toArray();
                            var itemCount = items.length;
                            for (let i = 0; i < itemCount; i++) {
                                var itemClass = items[i].getAttribute('class');
                                itemClass = itemClass.replace('carousel-1', this.data.guid);
                                items[i].setAttribute('class', itemClass);
                            }

                            var controls = this.element.find('.carousel-control').toArray();
                            var controlCount = controls.length;
                            for (let i = 0; i < controlCount; i++) {
                                controls[i].setAttribute('data-bs-target', '#' + this.data.guid);
                            }

                            var carouselStyle = this.element.findOne('style');
                            var styleHtml = carouselStyle.getHtml();
                            styleHtml = styleHtml.replace(/carousel-1/g, this.data.guid);
                            carouselStyle.setHtml(styleHtml);                            
                        } else {
                            
                        }

                        this.on('ready', function () {
                            runHelper();
                        });                        
                    },
                    // Prepare the data
                    data: function () {                        
                        if (this.data.ok == 'ok') {
                            var elem = this.element;
                            var styleElem = elem.findOne('style');
                            var styleHtml = styleElem.getHtml();
                            styleHtml = styleHtml.replace(/height:.*px/g, 'height: ' + this.data.carouselHeight);
                            styleElem.setHtml(styleHtml);     
                            //CKEDITOR.instances.e1.updateElement();
                            this.data.ok = '';
                        }                                                           
                    }
                }
            );

            function guid() {
                let s4 = () => {
                    return Math.floor((1 + Math.random()) * 0x10000)
                        .toString(16)
                        .substring(1);
                }
                //return id of format 'carousel'-'aaaa'-'aaaa'-'aaaa'-'aaaaaaaaaaaa'
                return 'carousel' + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
            }
        }

    }
    );

})();
