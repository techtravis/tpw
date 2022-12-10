CKEDITOR.dialog.add('jqcarousel', function (editor) {

    return {
        title: 'Create a Carousel',
        minWidth: 200,
        minHeight: 100,
        onShow: function () {
            //debugger;
            // Detect if we are on a Carousel
             var command = this.getName();

            var cHeight = this.getContentElement('info', 'carouselHeight');
            if (command == 'jqcarousel') {

            }
        },
        contents: [
            {
                id: 'info',
                label: 'Info',
                accessKey: 'I',
                elements: [

                    {
                        id: 'carouselHeight',
                        type: 'select',
                        label: 'Carousel Height',
                        items: [
                            ['200px', '200px'],
                            ['250px', '250px'],
                            ['300px', '300px'],
                            ['350px', '350px'],
                            ['400px', '400px'],
                            ['500px', '500px'],
                            ['600px', '600px'],
                            ['800px', '800px'],
                        ],
                        setup: function (widget) {
                            this.setValue(widget.data.carouselHeight || '');
                        },
                        commit: function (widget) {
                            var height = this.getValue();
                            widget.setData('carouselHeight', height);
                            widget.setData('ok', 'ok');
                        }
                    }
                ]
            }
        ],
        onOk: function () {
            var dialog = this;
        }
    };
});
