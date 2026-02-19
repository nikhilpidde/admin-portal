/*****************************
*
* Example Biometric Widget
*
* Uses Microphone.js class to get audio and pass it to Armorvox.js class. 
* Controls aspects of user interaction during the process and draws progress bar.
*
* It is not intended to be used in production! It is not secure since there is no server-side check of a legitimate browser-side client.
*
*/


function widgetSetup(id, av, prompts, isEnrol=false, prints={name:'digit', fa_rate:0.1, al_rate:0.1}, maxAttempts=2) {
	
	var failedAttempts = 0;
	var currentStep = 0;
	var maxSteps = prompts.length;
	var wavs_array = [];
	var handler = isEnrol?handleEnrol:handleVerify;
	
	if (prints.constructor !== Array) {
		prints = [prints];
	}
	
	prints.forEach((print) => {
		print.name = typeof print.name != 'string'?'digit':print.name;
		print.fa_rate = typeof print.fa_rate != 'number'?0.1:print.fa_rate;
		print.al_rate = typeof print.al_rate != 'number'?0.1:print.al_rate;
	});
	
	
	var mic = new Microphone(canvas_tag, select_tag);
	startListening();
	
	function startListening() {
		var listening = mic.startListening();
	
		listening.then((stream) => {
			console.log('Got stream:', stream);

			// For Desktops
			//record_button.addEventListener('mousedown', startRecording);
			//mic_widget.addEventListener('mouseup', stopRecording);
			//mic_widget.addEventListener('mouseleave', stopRecording);
			
			record_button.addEventListener('click', recodingHandler);

			// For iPhone
			record_button.addEventListener('touchstart', startRecording);
			mic_widget.addEventListener('touchend', stopRecording);

			// For Mobile Android
			record_button.addEventListener('pointerdown', startRecording);
			mic_widget.addEventListener('pointerup', stopRecording);
			mic_widget.addEventListener('pointerleave', stopRecording);

			// For Keyboard trigger
			mic_widget.tabIndex = 0;
			mic_widget.addEventListener('keypress', startRecording);
			mic_widget.addEventListener('keyup', stopRecording);
			mic_widget.addEventListener('mouseover', (e) => {mic_widget.focus();});
			
			
		    showMessage("ok");
		}).catch((e) => {
			console.log('error getting stream:', e.message);
			 if(e.message =="Permission denied")
						showMessage("permission");
		});
	}
	
	function recodingHandler(){
		console.log("record handler")
		if (record_button.value == "Record")
                start()
            else
                stop()
		
	}
	
	function start() {
		record_button.value = "stop";
		record_button.classList.remove('blue_record_button');
		record_button.classList.add('red_record_button');
		$('#waveImg1').hide();
		 canvas_tag.style.display = "block";
		
		record_button.classList.add('active');
		mic.startRecording();
		showMessage("progress");
	}
	
	function stop() {
		record_button.value = "Record";
		record_button.classList.remove('red_record_button');
		record_button.classList.add('blue_record_button');
		$('#waveImg1').show();
		showMessage("wait");

		 canvas_tag.style.display = "none";
		
		record_button.classList.remove('active');
		if (!mic.showEffect) {
			// its not recording at the moment
			return;
		}
	
		//record_box.classList.add('hidden');
		var recording = mic.stopRecording();
		
		recording.then((wav) => {
			handler(wav);
		}).catch((e) => { 
			console.log(e); 
			//record_box.classList.remove('hidden');
			showMessage(e);
		});
	}
	
	function qaCheckPasses(qa) {
		switch (qa[1]) {
			case '<=': return parseFloat(qa[0]) <= parseFloat(qa[2]);
			case '<': return parseFloat(qa[0]) < parseFloat(qa[2]);
			case '>=': return parseFloat(qa[0]) >= parseFloat(qa[2]);
			case '>': return parseFloat(qa[0]) < parseFloat(qa[2]);
			default: return false;
		}
	}
	
	function handleEnrol(wav) {
		// The wav audio is sent to a server to check for quality
		
		var print = prints[currentStep % prints.length];
		
		if('ti' == print.name){
			var phrase = null;
		}
		else{
			var phrase = getSpaced(prompts[currentStep]);
		}


		av.check(prints[0].name, wav.blob, phrase)
		.then((result) => {
			console.log(result);

			//record_box.classList.remove('hidden');
			switch (result.condition) {
			case 'QAFAILED':
				handleQAFAILED(result, 'enrol');
				break;
			case 'GOOD':
				drawPromptAndProgress(++currentStep, maxSteps);
				
				if (currentStep < maxSteps) {
					showMessage('ok');
					wavs_array.push(wav.blob);
				} else {
					/*
						A better implementation would not send this audio to the authentication server again for enrolment
						(it has already been sent for quality check).
						Instead, the server would hold it in a session object, ready for the signal to enrol it.
						However, we can't do that since this example is only connecting to the stateless ArmorVox server
					*/
					
					// The following enrols multiple prints. 
					// Each print in the prints array is enrolled
					var fn = (print) => {
						console.log("All utterances finished");
						console.log("Deleting user's previous "+print.name+" enrollment data");
						av.delete(id, 'ti');
						return av.enrol(id, print.name, wavs_array)
					}
					
					var actions = prints.map(fn);
					var results = Promise.all(actions);
					
					results.then((result) => {
						console.log(result);
						showMessage('complete');
					})
					.catch((result) => {
						console.log('error:'+result);
						showMessage('error');
					});		
				}
				break;
			default:
				showMessage('error');
			}
		})
		.catch((result) => {
			console.log('error:'+result);
			showMessage('error');
		});
	}
	
	function handleQAFAILED(result, mode = 'enrol') {
		var min_frames_qa = result.utterance[mode+'.qa.min_frames'];
		var phrase_prob_qa = result.utterance[mode+'.qa.phrase_fa_prob'];
		
		if (min_frames_qa && !qaCheckPasses(min_frames_qa)) {
			showMessage('too_quiet');
		} else if (phrase_prob_qa && !qaCheckPasses(phrase_prob_qa)) {
			showMessage('bad_phrase');
		} else {
			showMessage('error');
		}
	}
	
	function handleVerify(wav) {
		// The wav audio is sent to a server to verify
		var print = prints[currentStep % prints.length];

		if('ti' == print.name){
			var phrase = null;
		}
		else{
			var phrase = getSpaced(prompts[currentStep]);
		}
		
		av.verify(id, print.name, wav.blob, phrase, print.al_rate).then((result) => {
			console.log(result);
			//record_box.classList.remove('hidden');
			
			switch (result.condition) {
			
			case 'QAFAILED':
				handleQAFAILED(result, 'verify');
				break;
				
			case 'FAIL':
			case 'GOOD':
				/*
					A better implementation would not leave the security of the system in the browser-client!!!
					This must be done in the server - and the client just be told whether to verify again
				*/
				
				if (result.impostor_prob > print.fa_rate) {
					failedAttempts++;
					if (failedAttempts < maxAttempts) {
						showMessage('failed');
					} else {
						showMessage('complete_failed');
						currentStep = maxSteps;
						drawPromptAndProgress(currentStep, maxSteps, '#F44');
					}
				} else {
					failedAttempts = 0;
					drawPromptAndProgress(++currentStep, maxSteps);
					
					if (currentStep < maxSteps) {
						showMessage('ok');
					} else {
						showMessage('complete');
					}
				}
				break;
			default:	
				showMessage('error');
			}
		})
		.catch((result) => {
			console.log('error:'+result);
			showMessage('error');
		});
	}
	
	
	 function showMessage(e) {

        var error_msg = "";

        switch (e) {

            case 'failed': error_msg = "Your authentication attempt failed."; break;
            case 'error': error_msg = "There was an error."; break;
            case 'click': error_msg = ""; break;
            case 'too_short': error_msg = "The audio was too short.   You may need to speak more slowly."; break;
            case 'too_quiet': error_msg = "The audio was too quiet.   Check your mic settings."; break;
            case 'bad_phrase': error_msg = "The phrase didn't sound right."; break;
            case 'success': error_msg = "The voice was captured successfully"; break;
            case 'progress':  error_msg = "Recording started ....  Click the mic icon to stop recording"; break;
			case 'permission': error_msg = "Microphone permission is denyed."; break;
			case 'read_prompt': error_msg = "Click the mic icon and speak"; break;
			case 'ok': error_msg = "Click the mic icon and speak"; break;
			case 'wait': error_msg = "Wait..."; break;
            default:
        }

        switch (e) {
            case 'complete':
                error_msg = "Your enrolment was successful.";
				document.getElementById("instruction_box").classList.add('no-show'); 
				finish_button.classList.remove('no-show'); 
                break;

            case 'complete_failed':
                error_msg = "Your enrolment was not successful.";
                break;
        }

        document.getElementById("messageBox").innerHTML = error_msg;
    }
	/*function showMessage(e) {
		document.querySelectorAll('.no-show').forEach((el) => {el.classList.remove('show');});
		message_box.classList.remove('alert');
		
		switch (e) {
		
			case 'failed':  failed_message.classList.add('show'); message_box.classList.add('alert'); break;
			case 'error':  error_message.classList.add('show'); message_box.classList.add('alert'); break;
			case 'click': dont_click.classList.add('show'); message_box.classList.add('alert'); break;
			case 'too_short': too_short.classList.add('show'); message_box.classList.add('alert'); break;
			case 'too_quiet': too_quiet.classList.add('show'); message_box.classList.add('alert'); break;
			case 'bad_phrase': bad_phrase.classList.add('show'); message_box.classList.add('alert'); break;
			 case 'progress':  progress.classList.add('show'); message_box.classList.add('alert'); break;
			case 'permission': permission.classList.add('show'); message_box.classList.add('alert');break; 
			case 'read_prompt': read_prompt.classList.add('show'); message_box.classList.add('alert'); break;
			case 'ok': read_prompt.classList.add('show'); message_box.classList.add('alert'); break;
			default: 
		}
		
		
		
		switch (e) {
			case 'complete': 
				record_button.classList.add('no-show'); 
				finish_button.classList.remove('no-show'); 
				select_box.classList.add('hidden');
				read_prompt.classList.remove('show'); 
				complete_message.classList.add('show'); 
				break;
				
			case 'complete_failed': 
				record_button.classList.add('no-show'); 
				finish_button.classList.remove('no-show'); 
				select_box.classList.add('hidden');
				read_prompt.classList.remove('show'); 
				complete_failed_message.classList.add('show'); 
				break;
		}
	}*/
	
	function drawPromptAndProgress(currentProgress, maxProgress, fill_color='#4A4') {
	
		// Set the prompt text
		if (currentProgress < maxProgress) {
			var prompt = numBreak(prompts[currentProgress]);
			
			prompt_text.innerHTML = prompt;
		} else {
			prompt_text.innerHTML = '&nbsp;';
		}
	
		// Prepare canvas
		var canvas = canvas_progress;
		
		var canvasCtx = canvas.getContext('2d');
		const radius = 8;
		
		const inset = 20;
		const offset = radius*2;
		const width = canvas.width - (offset * 2) - (inset * 2);
		const diff = width / (maxProgress - 1);
		const y = canvas.height / 2;
		var x = offset + inset;

		canvasCtx.strokeStyle = 'black';
		canvasCtx.lineWidth  = 2;
		canvasCtx.fillStyle = fill_color;
		
		// Draw left inset
		canvasCtx.beginPath();
		canvasCtx.moveTo(0,y);
		canvasCtx.lineTo(inset+offset-radius, y);
		canvasCtx.stroke();
			  
		// Draw the circles
		for (var i = 1; i <= maxProgress; i++, x+=diff) {
		  canvasCtx.beginPath();
		  canvasCtx.arc(x, y, radius, 0, 2 * Math.PI);
		  canvasCtx.stroke();
		  if (i <= currentProgress) {
			canvasCtx.fill();
		  }
		  
			// Draw the connecting lines
			if (i < maxProgress) {
			  canvasCtx.beginPath();
			  canvasCtx.moveTo(x+radius,y);
			  canvasCtx.lineTo(x+diff-radius, y);
			  canvasCtx.stroke();
		  }
		}
		
		// Draw right inset
		
		canvasCtx.beginPath();
		canvasCtx.moveTo(canvas.width-inset-radius,y);
		canvasCtx.lineTo(canvas.width, y);
		canvasCtx.stroke();
			  
	}
	
	showMessage('ok');
	drawPromptAndProgress(0, maxSteps);
	
	
	// Set the pen color of the audio-feedback canvas
	canvas_tag.getContext('2d').strokeStyle = '#000';
}


function getRandomNumber() {
  var pool = ["1", "2", "3", "4", "5", "6", "7", "8", "9"];
  shuffleArray(pool);
  return pool.join('');
}

function shuffleArray(array) {
    for (let i = array.length - 1; i > 0; i--) {
        let j = Math.floor(Math.random() * (i + 1));
        [array[i], array[j]] = [array[j], array[i]];
    }
}

function numBreak(number) {

	if (typeof number == 'object') {
		number = number.text;
	}

	if (number.length < 6) {
		return number;
	}

	var loc = 3;
	if (number.length % 3 > 0) {
		loc = 4;
	}

	return number.substring(0,loc) + numBreak(number.substring(loc));
}

function getSpaced(number) {
	if (typeof number == 'object') {
		if (number.is_english == false) return null;
		number = number.text;
	}
	
	return number.split('').join(" ");
}

function getRequestParameter(name){
   if(name=(new RegExp('[?&]'+encodeURIComponent(name)+'=([^&]*)')).exec(location.search))
      return decodeURIComponent(name[1]);
}
