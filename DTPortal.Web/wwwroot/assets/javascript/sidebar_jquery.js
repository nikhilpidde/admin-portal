
            

    $(document).on('click','#sidebar ul li',function(){

        
        $('#sidebar ul li').removeClass('active')
      
        // $('#sidebar ul li ul li a').removeClass('active')
        $(this).addClass("active")
        // $(this' ul li ul li a').addClass('active')
        
    });
    
    $(document).on('click','#sidebar ul li ul li a',function(){
        
        
        $('#sidebar ul li ul li a').removeClass('active')
        $(this).addClass("active")
        // $('#sidebar ul li ul li a').removeClass('active')

    });
    

       $( "#sidebar ul li" ).on( "click", function() {

           $('#sidebar ul li ul li a').removeClass('active')
     });

     $(document).on('click','#sidebar.active ul li ul',function(){
        
        $(this).css("background-color", "rgb(0, 0, 0)")
        $(this).off();
        // $(this).removeClass('background-color');
        
        // if ($(this).css('background-color', "rgb(0,0,0)")) {
           
        //   } else {
        //     $(this).css('background-color',"rgb(30,30,30)");
        //   }
    

    });

    $(document).on('click','#sidebar.active ul li ul',function(){
        
        // $('#sidebar.active ul li ul').css("background-color", "rgb(30, 30, 30)")
        $('#sidebar.active ul li ul').off('click');
        
    });

    $(document).on('click','#sidebar.active ul li .dashColSubmenu',function(){
        
        $(this).css("background-color", "rgb(0, 0, 0)")
       
    });

    $( "#sidebar.active ul li " ).hover(function() {
        $("#sidebar.active ul li ul").css('background-color',"rgb(30,30,30)");
        // $(this).removeClass('.dashColSubmenu');
      });

   

 