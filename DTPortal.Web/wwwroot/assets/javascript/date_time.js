
    document.getElementById("para1").innerHTML = formatAMPM();
    document.getElementById("para2").innerHTML = formatAMPM();
    document.getElementById("para3").innerHTML = formatAMPM();
    document.getElementById("para4").innerHTML = formatAMPM();
    document.getElementById("para5").innerHTML = formatAMPM();

    function formatAMPM() {
      var d = new Date(),
              minutes = d.getMinutes().toString().length == 1 ? '0'+d.getMinutes() : d.getMinutes(),
              hours = d.getHours().toString().length == 1 ? '0'+d.getHours() : d.getHours(),
              ampm = d.getHours() >= 12 ? 'pm' : 'am',
              months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'],
              days = ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'];
              return hours+':'+minutes+ampm+' '+days[d.getDay()]+' '+d.getDate()+' '+months[d.getMonth()]+' '+d.getFullYear();
             }


// document.getElementById("para2").innerHTML = formatAMPM();

// function formatAMPM() {
//   var d = new Date(),
//           minutes = d.getMinutes().toString().length == 1 ? '0'+d.getMinutes() : d.getMinutes(),
//           hours = d.getHours().toString().length == 1 ? '0'+d.getHours() : d.getHours(),
//           ampm = d.getHours() >= 12 ? 'pm' : 'am',
//           months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'],
//           days = ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'];
//           return hours+':'+minutes+ampm+' '+days[d.getDay()]+' '+d.getDate()+' '+months[d.getMonth()]+' '+d.getFullYear();
//          }




// document.getElementById("para3").innerHTML = formatAMPM();
// function formatAMPM() {
//   var d = new Date(),
//           minutes = d.getMinutes().toString().length == 1 ? '0'+d.getMinutes() : d.getMinutes(),
//           hours = d.getHours().toString().length == 1 ? '0'+d.getHours() : d.getHours(),
//           ampm = d.getHours() >= 12 ? 'pm' : 'am',
//           months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'],
//           days = ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'];
//           return hours+':'+minutes+ampm+' '+days[d.getDay()]+' '+d.getDate()+' '+months[d.getMonth()]+' '+d.getFullYear();
//          }
