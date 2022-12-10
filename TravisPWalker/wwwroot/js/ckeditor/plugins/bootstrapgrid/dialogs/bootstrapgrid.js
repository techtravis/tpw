CKEDITOR.dialog.add( 'bootstrapgrid', function( editor ) {
  function uniStr(unicode) {
        return String.fromCharCode(parseInt(unicode,16));      
  }
  function countInstancesOf(letter, sentence) {
//debugger;
      var count = 0;

      for (var i = 0; i < sentence.length; i++) {
        if (sentence.charAt(i) === letter) {
          count += 1;
        }
      }
      return count;
  }
  return {
    title: 'Bootstrap Grid',
    minWidth: 300,
    minHeight: 150,
    onShow: function() {
//debugger;
      // Detect if we are on a bootstrapgrid 
      var selection = editor.getSelection(),
        ranges = selection.getRanges();
      var command = this.getName();

        var rowsInput = this.getContentElement('info', 'rowLayout');
      if (command == 'bootstrapgrid') {
        var grid = selection.getSelectedElement();
        // Enable or disable row and cols.
        if (grid) {
          this.setupContent(grid);
            rowsInput && rowsInput.disable();
        }
      }
    },
    contents: [
      {
        id: 'info',
        label: 'Info',
        accessKey: 'I',
        elements: [
          
          {
            id: 'rowLayout',
            type: 'select',
            label: 'Row Layout',
            items: [
                [uniStr('00A0') + uniStr('25AC') + uniStr('25AC') + uniStr('25AC') + uniStr('25AC') + uniStr('25AC') + uniStr('25AC') + uniStr('25AC') + uniStr('00A0'), '12c'],                
                [uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('25AC') + uniStr('25AC'), '4s-4c-4s'],
                [uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('25AC'), '5s-2c-5s'],
                [uniStr('25AC') + uniStr('25AC') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('25AC') + uniStr('25AC') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('25AC') + uniStr('25AC'), '4c-4c-4c'],
                [uniStr('25AC') + uniStr('25AC') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('25AC') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0')+ uniStr('00A0')+ uniStr('00A0') + uniStr('25AC') + uniStr('25AC'), '4c-1s-2c-1s-4c'],
                [uniStr('25AC') + uniStr('25AC') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0')  + uniStr('25AC') + uniStr('25AC'), '4c-4s-4c'],
                [uniStr('25AC') + ' ' + uniStr('25AC') + ' ' + uniStr('25AC') + ' ' + uniStr('25AC') + ' ' + uniStr('25AC') + ' ' + uniStr('25AC'), '2c-2c-2c-2c-2c-2c'],
                [uniStr('25AC') + ' ' + uniStr('25AC') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0')  + uniStr('25AC') + ' ' + uniStr('25AC'), '2c-2c-4s-2c-2c'],
                [uniStr('25AC') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('25AC') + uniStr('25AC') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('25AC'), '2c-2s-4c-2s-2c'],
                [uniStr('25AC') + uniStr('25AC') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('25AC') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('25AC') + uniStr('25AC'), '3c-2s-2c-2s-3c'],
                [uniStr('25AC') + uniStr('25AC') + uniStr('25AC') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('25AC') + uniStr('25AC') + uniStr('25AC'), '5c-2s-5c'],
                [uniStr('25AC') + uniStr('25AC') + uniStr('25AC') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('25AC'), '6c-4s-2c'],
                [uniStr('25AC') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('00A0') + uniStr('25AC') + uniStr('25AC') + uniStr('25AC'), '2c-4s-6c']
                
            ],
            setup: function (widget) {
                this.setValue(widget.data.rowLayout || '');
            },
            commit: function (widget) {
                var columnCount = Number(countInstancesOf('c', this.getValue()));
                widget.setData('rowlayout', this.getValue());
                widget.setData('colCount', columnCount);
                widget.setData('rowCount', 1);
            }
          }
         
        ]
      }
    ],
  };
});
