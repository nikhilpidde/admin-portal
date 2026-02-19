//jquery for blue background



//jquery for yellow background

$(document).ready(function(){

    $(".content-wrapper .dropdown .dropdown-menu .yellow-image").on('click',function(){
        console.log("color-testing yellow");
        $("body .background-image").removeClass('background-image-red').addClass('background-image');
        $("body .background-image").removeClass('background-image-blue').addClass('background-image');

        $('head').append('<link rel="stylesheet" href="/assets/externalCss/yellow.css" type="text/css" />');
        
        console.log("color yellow added");
    });  
    
});

//jquery for red backgorund

$(document).ready(function(){
     $(".content-wrapper .dropdown .dropdown-menu .red-image").on('click',function(){
        console.log("color-testing red");
         $("body .background-image").removeClass('background-image-blue').addClass('background-image-red');
         $("body .background-image").removeClass('background-image').addClass('background-image-red');


        $('head').append('<link rel="stylesheet" href="/assets/externalCss/red.css" type="text/css" />');
        console.log("color red added");
    }); 
});

