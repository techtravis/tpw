(function(){
  CKEDITOR.plugins.add('bootstrapgrid', {
      requires: 'widget,dialog',
      icons: 'bootstrapgrid',
      init: function(editor) {
      CKEDITOR.dialog.add('bootstrapgrid',  this.path + 'dialogs/bootstrapgrid.js');

       editor.addContentsCss( this.path + 'styles/editview.css');
       // Add widget
       editor.ui.addButton('bootstrapgrid', {
         label: 'Create Grid Row',
         command: 'bootstrapgrid',
         icon: this.path + 'icons/bootstrapgrid.png'
       });
       editor.widgets.add('bootstrapgrid',
         {
           allowedContent: 'div(!bootstrapgrid);div(!row,!row-*);div(!col-*-*);div(!content)',
           requiredContent: 'div(bootstrapgrid)',
           parts: {
             bootstrapgrid: 'div.bootstrapgrid',
           },
           editables: {
             content: '',
           },
           template:
                   '<div class="bootstrapgrid">' +
                   '</div>',
           dialog: 'bootstrapgrid',
           defaults: {
           },
           // Before init.
           upcast: function(element) {
             return element.name == 'div' && element.hasClass('bootstrapgrid');
           },
           // initialize
           // Init function copy/paste hits this.
           init: function() {
             var rowNumber= 1;
             var rowCount = 1; //this.element.getChildCount();
             var rowLayout = '12c';
             var colCount = 12;
             for (rowNumber; rowNumber <= rowCount;rowNumber++) {
                 this.createEditableView(colCount, rowNumber, rowLayout);
             }
           },
           // Prepare the data
           data: function() {
             if (this.data.colCount && this.element.getChildCount() < 1) {
               var colCount = this.data.colCount;
               var rowCount = this.data.rowCount;   
               var rowLayout = this.data.rowlayout;
                 var row = this.parts['bootstrapgrid'];
                 //debugger;
               for (var i= 1;i <= rowCount;i++) {
                 this.createGrid(rowLayout, row, i, colCount);
               }
             }
           },
           
           // Create grid
           createGrid: function(rowLayout, row, rowNumber, colCount) {
               var content = '<div rowLayout="' + rowLayout +'" class="row row-' + rowNumber + '">';
                    if(rowLayout == '12c') {
                        content = content +
                                   '<div class="col col-md-12 offset-md-0 col-lg-12 offset-lg-0 col-xl-10 col-xl-offset-1">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-lg-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 1 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +                                   
                                   '</div>';
                    }
                    else if(rowLayout == '4s-4c-4s') {
                        content = content +
                                   '<div class="col col-md-6 offset-md-3 col-lg-4 offset-lg-4">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-lg-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 1 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +                                   
                                   '</div>';
                    }
                    else if(rowLayout == '5s-2c-5s') {
                        content = content +
                                   '<div class="col col-md-4 offset-md-4 col-lg-2 offset-lg-5">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-lg-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 1 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +                                   
                                   '</div>';
                    }
                    else if(rowLayout == '4c-4c-4c') {
                        content = content +
                                   '<div class="col col-md-4 col-lg-4">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 1 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-4 col-lg-4">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-lg-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 2 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +                                   
                                   '</div>' +
                                   '<div class="col col-md-4 col-lg-4">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="pull-right-lg">' +
                                   '            <div class="col-md-12">' +
                                   '                <div class="content pull-right-lg"">' +
                                   '                    <p>Col 3 content area</p>' +
                                   '                </div>' +
                                   '            </div>' +
                                   '        </div>' + 
                                   '    </div>' +
                                   '</div>';
                    }
                    else if(rowLayout == '4c-1s-2c-1s-4c') {
                        content = content +
                                   '<div class="col col-md-5 col-lg-4">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 1 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-2 col-lg-2 offset-lg-1">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-lg-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 2 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +                                   
                                   '</div>' +
                                   '<div class="col col-md-5 col-lg-4 offset-lg-1">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="pull-right-lg">' +
                                   '            <div class="col-md-12">' +
                                   '                <div class="content pull-right-lg"">' +
                                   '                    <p>Col 3 content area</p>' +
                                   '                </div>' +
                                   '            </div>' +
                                   '        </div>' + 
                                   '    </div>' +
                                   '</div>';
                    }
                    else if(rowLayout == '4c-4s-4c') {
                        content = content +
                                   '<div class="col col-md-5 col-lg-4">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 1 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-5 offset-md-2 col-lg-4 offset-lg-4">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="pull-right-lg">' +
                                   '            <div class="col-md-12">' +
                                   '                <div class="content pull-right-lg"">' +
                                   '                    <p>Col 2 content area</p>' +
                                   '                </div>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>';
                    }
                    else if(rowLayout == '2c-2c-2c-2c-2c-2c') {
                        content = content +
                                   '<div class="col col-md-4 col-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 1 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-4 col-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 2 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-4 col-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 3 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-4 col-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 4 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-4 col-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 5 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-4 col-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 6 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>';
                    }
                    else if(rowLayout == '2c-2c-4s-2c-2c') {
                        content = content +
                                   '<div class="col col-md-3 col-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 1 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-2 col-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 2 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-3 offset-md-2 col-lg-2 offset-lg-4">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 3 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-2 col-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 4 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>';
                    }
                    else if(rowLayout == '2c-2s-4c-2s-2c') {
                        content = content +
                                   '<div class="col col-md-3 col-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 1 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-4 offset-md-1 col-lg-4 offset-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 2 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-3 offset-md-1 col-lg-2 offset-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 3 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>';
                    }
                    else if(rowLayout == '3c-2s-2c-2s-3c') {
                        content = content +
                                   '<div class="col col-md-4 col-lg-3">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 1 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-2 offset-md-1 col-lg-2 offset-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 2 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-4 offset-md-1 col-lg-3 offset-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 3 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>';
                    }
                    else if(rowLayout == '5c-2s-5c') {
                        content = content +
                                   '<div class="col col-md-5 col-lg-5">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 1 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-5 offset-md-2 col-lg-5 offset-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="pull-right-lg">' +
                                   '           <div class="col-md-12">' +
                                   '               <div class="content pull-right-lg">' +
                                   '                 <p>Col 2 content area</p>' +
                                   '               </div>' +
                                   '           </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>';
                    }
                    else if(rowLayout == '6c-4s-2c') {
                        content = content +
                                   '<div class="col col-md-6 col-lg-6 ">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 1 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-2 offset-md-4 col-lg-2 offset-lg-4">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="pull-right-lg">' +
                                   '            <div class="col-md-12">' +
                                   '                <div class="content pull-right-lg"">' +
                                   '                    <p>Col 2 content area</p>' +
                                   '                </div>' +
                                   '            </div>' +
                                   '        </div>' +  
                                   '    </div>' +
                                   '</div>';
                    }
                    else if(rowLayout == '2c-4s-6c') {
                        content = content +
                                   '<div class="col col-md-2 col-lg-2">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="col-md-12">' +
                                   '            <div class="content">' +
                                   '                <p>Col 1 content area</p>' +
                                   '            </div>' +
                                   '        </div>' +
                                   '    </div>' +  
                                   '</div>' +
                                   '<div class="col col-md-6 offset-md-4 col-lg-6 offset-lg-4">' +
                                   '    <div class="row outer-content">' +
                                   '        <div class="pull-right-lg">' +
                                   '            <div class="col-md-12">' +
                                   '                <div class="content pull-right-lg"">' +
                                   '                    <p>Col 2 content area</p>' +
                                   '                </div>' +
                                   '            </div>' +
                                   '        </div>' + 
                                   '    </div>' +  
                                   '</div>';
                    }
             content =content + '</div>';
             row.appendHtml(content);
//debugger;
             this.createEditableView(colCount,rowNumber,rowLayout);
           },
           // Create editable.
           createEditableView: function(colCount,rowNumber,rowLayout) {
//debugger;
             for (var i = 1; i <= colCount; i++) {
               this.initEditable( 'content'+ rowNumber + i, {
                  selector: '.row-'+ rowNumber +' > div:nth-child('+ i +') div.content'
                } );
              }
            }
          }
        );
      }
    }
  );

})();
