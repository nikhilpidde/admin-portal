/**************************
*
* Microphone Class
* (c) Auraya Systems 2019
*
* Example use:
* Assume a 'record' button element, and a canvas element.


		var mic = new Microphone(canvas);	
		var listening = mic.startListening(); // At this point, user might be asked to allow audio for this site. 
		
		
		listening.then(() => {
			record.addEventListener('mousedown', () => {mic.startRecording();});
			document.addEventListener('mouseup', () => {
				mic.stopRecording().then((wav) => {
					// do something with wav file
					var data = new FormData();
					data.append('utterance', wav.blob);
				});
			});
		}


*
*
***************************/

/* Compatibility across browsers */
var AudioContext = window.AudioContext || window.webkitAudioContext;

/**
* Creates a Microphone used to make audio recordings. 
* Supply an optional canvas element to display visual feedback
* Supply an optional select element to populate options with available microphones on the system
*
* @constructor
* @param {HTMLCanvasElement} Optional canvas element to display visual feedback
* @param {HTMLCanvasElement} Optional select element to populate with available microphones
* @param {number} Time in milliseconds to wait for recording to end
* @param {number} Time in milliseconds required before accepting recording
* @param {boolean} Indicates if WAV audio is encoded as ulaw rather than PCM
*/
function Microphone(canvas = null, select = null, releaseTime = 400, minHoldTime = 1000, isConvertToUlaw = true) {
	this.canvas = canvas;
	this.select = select;
	this.releaseTime = releaseTime;
	this.minHoldTime = minHoldTime;
	this.clickTime = 100;
	this.audioBuffer = [];
	this.isRecording = false;
	this.showEffect = false;
	this.stopRecordingTime = 0;
	this.startRecordingTime = 0;
	this.speechElapsedTime = 0;
	this.isConvertToUlaw = isConvertToUlaw;
}

/**
* Attempts to start the audio stream (the red dot appears in tab). This request may be rejected by the browser (or user).
* It can take half a second or so to start up, so should be called well before user presses record.
*
* Rejected if the microphone is already listening to a stream.
*
* By default, a success calls setupInputSelect (since all the deviceIds and labels are then available).
* 
* @param {string} Optional id of microphone. This id can be determined by calling navigator.mediaDevices.enumerateDevices();
* @return {Promise} A successful promise indicates a functioning audio stream. 
*/
Microphone.prototype.startListening = function(deviceId = null, doInputSelect = true) {
	
	if (this.mediaStream && this.mediaStream.readyState != 'ended') {
		return new Promise((resolve, reject) => {
			reject('active');
		});
	}
	
	var constraints = { deviceId: deviceId };
	var promise = navigator.mediaDevices.getUserMedia({audio: constraints});
	
	return promise.then((stream) => {
		return new Promise((resolve, reject) => {
			try {	
				this.attachStream(stream);
				if (stream.active) {
					if (this.select != null && doInputSelect) {
						var track = stream.getTracks()[0];
						var settings = track.getSettings();
						this.setupInputSelect(settings.deviceId);
					}
					resolve(stream);
				} else {
					reject(e);
				}
			} catch (e) {
				reject(e);
			}	
		})
	}).catch ((err) => {
		return new Promise((resolve, reject) => {
			reject(err);
		});
	});
	
}

/**
* Stops listening to audio. (The red dot goes away).
*
*/
Microphone.prototype.stopListening = function() {
	if (this.mediaStream) {
		this.mediaStream.stop();
	}
}
/**
* Starts buffering audio. This only happens if startListening was successful.
*
*/
Microphone.prototype.startRecording = function () {
  console.log('ENTER startRecording');
  if (!this.isRecording) {
	  this.isRecording = true;
	  this.showEffect = true;
	  this.audioBuffer = [];
	  this.startTime = Date.now();
  }
}

/**
* Starts buffering audio. This only happens if startListening was successful.
* @return {Promise} A successful promise is passed an object containing a wav file {blob: wav_blob, wav: wav_array, sample_rate: sampleRate, miclabel: miclabel}
*/
Microphone.prototype.stopRecording = function () {
  console.log('ENTER stopRecording. buffer size:', this.audioBuffer.length);
  
  return new Promise((resolve, reject) => {
			
			if (!this.showEffect) {
				reject("not_recording");
			}
			
			this.stopTime = Date.now();
			this.elapsedTime = this.stopTime - this.startTime;
			this.clearEnergyEffect();
			this.showEffect = false;
			
			if (this.elapsedTime < this.clickTime) {
				reject("click");
			}
			
			if (this.elapsedTime < this.minHoldTime) {
				reject("too_short");
			}
			
			setTimeout(() => {
				this.isRecording = false;
				try {
					resolve(this.makeWav());
				} catch (err) {
					reject(err);
				}
			}, this.releaseTime);
		});
}

/**
* Inserts Microphone option into select element
*
* @private
* @param {MediaDevicesInfo}
* @param {array} Contains the list of seen groupIds - this prevents listing duplicate devices
* @param {number} Microphone number (in case labels are not available)
* @param {boolean} whether to set as the 'selected' option
*/
Microphone.prototype.makeOption = function(device, groups, i, selected = false) {
		var label = device.label != ''?device.label:"Microphone "+i;
		var node = document.createElement("option"); 
		node.value = device.deviceId;
		node.innerHTML = label;
		node.selected = selected;
		
		this.select.appendChild(node);
		if (device.groupId != '') {
			groups.push(device.groupId);
		}
	}

/**
* Redraws the list of Microphone options in the select tag. Attempts to select the correct option corresponding to 'deviceId'
*
* @param {string} A deviceId to set as selected option
*/	
Microphone.prototype.setupInputSelect = function (deviceId = null) {
	console.log('ENTER setupInputSelect');
	
	const STORAGE_PREFERRED_MIC = "preferred_mic";
	
	this.select.addEventListener('change', (e) => {
		localStorage.setItem(STORAGE_PREFERRED_MIC, e.target.selectedOptions[0].label);
		this.stopListening();
		this.startListening(e.target.value, false);
	});
	
	// attempt to reset to preferred device
	navigator.mediaDevices.enumerateDevices().then((devices) => {
		var preferredDevice = null;
		devices.forEach((device) => {
			if (device.kind == 'audioinput' && device.label == localStorage.getItem(STORAGE_PREFERRED_MIC) && deviceId != device.deviceId) {
				preferredDevice = device;
			}
		});
		if (preferredDevice != null) {
				this.stopListening();
				this.startListening(preferredDevice.deviceId, true);
		} else {
	
			this.select.innerHTML = '';

			navigator.mediaDevices.enumerateDevices()
			.then((devices) => {

				var i = 0;
				var groups = [];
				// First try to select to chosen device
				if (deviceId != null) {
					for (var device of devices) {
						if (device.kind == 'audioinput' && device.deviceId == deviceId) {
							this.makeOption(device, groups, ++i, true);
							break;
						}
					};
				}

				//  Then list all the other devices
				for (var device of devices) {
					console.log(device.kind + ": " + device.label + " id = " + device.deviceId + " groupId = " + device.groupId);
					if (device.kind == 'audioinput' && !groups.includes(device.groupId)) { 
						this.makeOption(device, groups, ++i, false);
					}
				};
			})
			.catch(function(err) {
			  console.log(err.name + ": " + err.message);
			});
			
		}
	});
	
	
}


/**
* Attaches the stream to a script processor node. This periodically sends audio to a processing function.
* @private
* @param {object} The stream
*/
Microphone.prototype.attachStream = function(stream) {
  console.log("ENTER attachStream");
  
  if (this.audioCtx == null) {
	this.audioCtx = new AudioContext();
	console.log('new AudioContext: sampleRate=' + this.audioCtx.sampleRate);
  
	this.recorderNode = this.audioCtx.createScriptProcessor(4096, 1, 1);
	this.recorderNode.connect(this.audioCtx.destination);
	
	this.recorderNode.onaudioprocess = (e) => {
	  this.onAudioProcess(e);
	}
  }
   
  this.stream = stream;
  this.mediaStream = stream.getTracks()[0];
  this.miclabel = this.mediaStream.label;
  
  var audioInput = this.audioCtx.createMediaStreamSource(stream);
  audioInput.connect(this.recorderNode);
}

/**
* Processes a chunk of audio by adding to a buffer. 
* This function is called periodically by the audio processor node.
* @private
* @param {object} The audio chunk
*/
Microphone.prototype.onAudioProcess = function(e) {
  //console.log('ENTER onAudioProcess:', this);
  if (this.isRecording) {    
    
	console.log('recording');
	var frames = e.inputBuffer.length; 
	console.log(frames);
	var array = e.inputBuffer.getChannelData(0).slice(); 
	this.audioBuffer.push(array);
	
  }
  
	if (this.showEffect && this.canvas != null) {
		var downsampledAudio = this.downsampleBufferFIR(e.inputBuffer);
		downsampledAudio = this.convertFloat32ToInt16(downsampledAudio);
		this.drawAmplitudeEffect(downsampledAudio);
	}

}

/**
* Attempts to draw audio samples onto the canvas member object. 
* This should be called periodically as part of buffering the audio.
* @private
* @param {array} The audio data
*/
Microphone.prototype.drawAmplitudeEffect = function (dataArray) {
  var canvasCtx = this.canvas.getContext('2d');
 
  canvasCtx.clearRect(0, 0, this.canvas.width, this.canvas.height);
  canvasCtx.beginPath();

  const bufferLength = dataArray.length;
  const sliceWidth = this.canvas.width * 1.0 / bufferLength;
  var x = 0;
  for (var i = 0; i < bufferLength; i++) {
    var v = (dataArray[i]  / 32767);
    v = v * 2; // how large the spikes
    var y = (v * this.canvas.height) + (this.canvas.height / 2.0);
    if (i === 0) {
      canvasCtx.moveTo(x, y);
    } else {

      canvasCtx.lineTo(x, y);
    }
    x += sliceWidth;
  }
  canvasCtx.stroke();
}

/**
* Clears the canvas. This should be called when the user releases stops recording.
* @private
*/
Microphone.prototype.clearEnergyEffect = function () {
	if (this.canvas != null) {
		var canvasCtx = this.canvas.getContext('2d');
		canvasCtx.clearRect(0, 0, this.canvas.width, this.canvas.height);
	}
}

Microphone.prototype.makeWav = function () {

  var subChunk2Id = new Int8Array([0x64, 0x61, 0x74, 0x61]); // data
  var sampleRate = this.audioCtx.sampleRate;
  var frameCount = 0;
  for (var i = 0; i < this.audioBuffer.length; i++) {
    frameCount += this.audioBuffer[i].length;
  }


  var arrayBuffer = this.audioCtx.createBuffer(1, frameCount, sampleRate);
  var frame = 0;
  var data = arrayBuffer.getChannelData(0);
  for (var i = 0; i < this.audioBuffer.length; i++) {
    var ab = this.audioBuffer[i];
    for (var j = 0; j < ab.length; j++) {
      data[frame] = ab[j];
      frame++;
    }
  }
  
  var audioBuffer8k = this.downsampleBufferFIR(arrayBuffer);

  var samples = this.convertFloat32ToInt16(audioBuffer8k);
  var wave = this.makeRIFF(samples); // create the wave file
  var blob = new Blob(wave);

  return {blob: blob, orig_data: wave, sample_rate: sampleRate, miclabel: this.miclabel};
}

/**
* Added a RIFF header to audio data.
* @private
* @param {array} 16bit 8kHz PCM audio sample array
* @param {boolean} whether to convert data to ulaw
* @return RIFF header audio byte array
*/
Microphone.prototype.makeRIFF = function (data) {
	
	var reducer = (a,c) => a + c.byteLength; // Lambda for calculating chunk size
	
	// RIFF Header 
	var riff = new Int8Array([0x52, 0x49, 0x46, 0x46]); // RIFF
	var format = new Int8Array([0x57, 0x41, 0x56, 0x45]); // WAVE
	
	// Default format is PCM 8KHz, 16bit
	var audioFormat = 1;
	var byteRate = 16000;
	var blockAlign = 2;
	var bitsPerSample = 16;

	// Convert to ulaw?
	if (this.isConvertToUlaw) {
		data = this.convertToUlaw(data);
		audioFormat = 7;
		byteRate = 8000;
		blockAlign = 1;
		bitsPerSample = 8;
	}  
	
	
	// Chunk1 specifies encoding details
	var audioFormat = new Uint16Array([audioFormat]);
	var numChannels = new Uint16Array([1]);
	var sampleRate = new Uint32Array([8000]);
	var byteRate = new Uint32Array([byteRate]);
	var blockAlign = new Uint16Array([blockAlign]);
	var bitsPerSample = new Uint16Array([bitsPerSample]);
  	
	var subChunk1 = []
		.concat(audioFormat)
		.concat(numChannels)
		.concat(sampleRate)
		.concat(byteRate)
		.concat(blockAlign)
		.concat(bitsPerSample);
	
	
	var subChunk1Id = new Int8Array([0x66, 0x6d, 0x74, 0x20]); // fmt 
	var subChunk1Size = new Uint32Array([subChunk1.reduce(reducer, 0)]);
	
	var chunk1 = []
		.concat(subChunk1Id)
		.concat(subChunk1Size)
		.concat(subChunk1);
  
  
	// Chunk2 is the data itself
	var subChunk2Id = new Int8Array([0x64, 0x61, 0x74, 0x61]); // data
	var subChunk2Size = new Uint32Array([data.byteLength]); // length

	var chunk2 = []
		.concat(subChunk2Id)
		.concat(subChunk2Size)
		.concat(data);
	
	// Calculate total size for RIFF
	var totalChunkSize = new Uint32Array([chunk1.reduce(reducer, 0) + chunk2.reduce(reducer, 0)]);
  
	return []
	
		// Header info for RIFF
		.concat(riff)
		.concat(totalChunkSize)
		.concat(format)
		.concat(chunk1)
		.concat(chunk2);	
}

/**
* Converts 16bit audio to ulaw
* @private
* @param {array} 16bit PCM audio sample array
* @return {array} ulaw byte array
*/
Microphone.prototype.convertToUlaw = function (samples) {
  var convertedSamples = new Int8Array(samples.length);
  var BIAS = 0x84;
  var CLIP = 32635;
  for (var i = 0; i < samples.length; i++) {
	  var sample = samples[i];

	  /* Get the sample into sign-magnitude. */
	  var sign = sample > 0?0:0x80;
	  if (sign != 0) sample = -sample;		/* get magnitude */
	  if (sample > CLIP) sample = CLIP;		/* clip the magnitude */

	  /* Convert from 16 bit linear to ulaw. */
	  sample = sample + BIAS;
	  
	  var ex_lookup = (sample >> 7) & 0xFF;
	  var exponent = exp_lut[ex_lookup];
	  var mantissa = (sample >> (exponent + 3)) & 0x0F;
	  var ulawbyte = ~(sign | (exponent << 4) | mantissa);
	  convertedSamples[i] = ulawbyte;
  }
  return convertedSamples;
}

/**
* Down-samples the audio from source rate to target rate.
*
* First, applies a low-pass filter
* Then, takes every Nth sample
*
* @private
* @param {array} 16bit PCM audio sample array (48kHz or 44.1kHz)
* @return {array} 16bit PCM audio sample array (8kHz)
*/
Microphone.prototype.downsampleBufferFIR = function (buffer) {
  var rate = 8000; // target rate is 8kHz

  var sampleRate = buffer.sampleRate; // source rate
  if (rate == sampleRate) {
    return buffer;
  }
  if (rate > sampleRate) {
    throw "downsampling rate should be smaller than original sample rate";
  }

  // First apply low-pass filter (FIR)
  
  // different FIR coefficients required for source/target sample-rate combinations
  var coeffs = firCoefficients[sampleRate];
  var firBuffer = Array.apply(null, Array(coeffs.length)).map(Number.prototype.valueOf, 0);
  var firBufferPos = 0;

  buffer = buffer.getChannelData(0);
  // apply FIR
  for (var i = 0; i < buffer.length; i++) {
    firBuffer[firBufferPos] = buffer[i];
    var total = 0;
    var firBufferPosTemp = firBufferPos;
    for (var j = 0; j < coeffs.length; j++) {
      total += firBuffer[firBufferPosTemp++] * coeffs[j];
      if (firBufferPosTemp == firBuffer.length) {
        firBufferPosTemp = 0;
      }
    }
    buffer[i] = total;
    firBufferPos++;
    if (firBufferPos == firBuffer.length) {
      firBufferPos = 0;
    }
  }

  // Then just pick every Nth sample value from filtered buffer
  var sampleRateRatio = sampleRate / rate;
  var newLength = Math.round(buffer.length / sampleRateRatio);
  var result = new Float32Array(newLength);
  var offsetResult = 0;

  while (offsetResult < result.length) {
    var offsetBuffer = Math.round(offsetResult * sampleRateRatio);
    while (offsetBuffer >= buffer.length) {
      offsetBuffer--;
    }
    result[offsetResult] = buffer[offsetBuffer];
    offsetResult++;
  }
  return result;
}

Microphone.prototype.convertFloat32ToInt16 = function (buffer) {
  var l = buffer.length;
  var ratio = 1; //8 / 44.1;
  var buf = new Int16Array(l * ratio);
  for (var i = 0; i < buf.length; i++) {
    buf[i] = Math.min(1, buffer[i / ratio]) * 0x7FFF;
  }
  return buf;
}

var exp_lut = [	 0,0,1,1,2,2,2,2,3,3,3,3,3,3,3,3,
				 4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,
				 5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,
				 5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,
				 6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,
				 6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,
				 6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,
				 6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,
				 7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
				 7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
				 7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
				 7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
				 7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
				 7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
				 7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
				 7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7];

var firCoefficients = { 

/*

FIR filter designed with
http://t-filter.appspot.com

sampling frequency: 48000 Hz

* 0 Hz - 3800 Hz
  gain = 1
  desired ripple = 5 dB
  actual ripple = 4.173247702836671 dB

* 4000 Hz - 24000 Hz
  gain = 0
  desired attenuation = -40 dB
  actual attenuation = -40.00542663589378 dB

*/




48000: [
  0.0037078853622880764,
  -0.0031818655616045488,
  -0.005002322929556022,
  -0.00783227343492291,
  -0.01113092508719082,
  -0.014359258966292856,
  -0.01697428458565556,
  -0.018485751067983044,
  -0.018536462203899054,
  -0.01697603432198143,
  -0.013906238412038235,
  -0.00968260511176664,
  -0.004866825267235354,
  -0.00013647892130397337,
  0.0038343528384597028,
  0.006498265087730188,
  0.007538459102064722,
  0.006931369436850068,
  0.004949593517303934,
  0.0021087251039519934,
  -0.0009376321564057613,
  -0.0035306816963085242,
  -0.005140336866112635,
  -0.0054719163046588574,
  -0.0045215943703137106,
  -0.0025690086034571235,
  -0.00010727558975114284,
  0.0022726954863922866,
  0.004014283934301812,
  0.004729404355044544,
  0.004274810366365638,
  0.0027887012091385795,
  0.000652193742215706,
  -0.0016041438833185786,
  -0.0034279259503435917,
  -0.004376950825652909,
  -0.004224827358782935,
  -0.0030158513710497044,
  -0.0010543000028954326,
  0.0011704366742285258,
  0.0031038502164729017,
  0.004262544722478894,
  0.004353063177238872,
  0.0033452774503363006,
  0.0014849095218848305,
  -0.000769666530166805,
  -0.0028537845954173893,
  -0.004238787550945243,
  -0.004563894233780151,
  -0.003728919286706744,
  -0.0019244495252580096,
  0.0004105056263627215,
  0.002693703163639939,
  0.004343677948935637,
  0.004926514748562088,
  0.00426804175421079,
  0.002505225803218639,
  0.00005655106722562884,
  -0.0024742024014937133,
  -0.004445439642947873,
  -0.005338523546370574,
  -0.004892162222141496,
  -0.003177852260521262,
  -0.000592938956035571,
  0.0022298500718601275,
  0.004576752685339087,
  0.005832464615232373,
  0.00563788548407116,
  0.003987537793798602,
  0.0012448820029745708,
  -0.0019376490537534373,
  -0.004739988345774312,
  -0.00645563243609975,
  -0.006583138132634261,
  -0.005022223773762522,
  -0.0020976627393834687,
  0.0015045074968363624,
  0.004892917783243419,
  0.007188042710323042,
  0.007746341353537545,
  0.006335564079358473,
  0.003214358509674901,
  -0.0009083430915649766,
  -0.0050288277265914194,
  -0.008088227364142088,
  -0.00923726202694145,
  -0.008064398618407978,
  -0.004727109371179884,
  0.00005455829982133947,
  0.005143955837753482,
  0.009249633156439159,
  0.011241980882152548,
  0.01045043513411174,
  0.006866325074436295,
  0.0011941417654284029,
  -0.005271185205391984,
  -0.010924703994865847,
  -0.014229780437888464,
  -0.014103485003942545,
  -0.010246397991601682,
  -0.0032661256223170043,
  0.005364854734696744,
  0.013593347998558365,
  0.019230199019771778,
  0.02047189461312385,
  0.016383751711732026,
  0.007232604034199318,
  -0.005417803990550028,
  -0.018891553934292114,
  -0.029848252908431856,
  -0.03491591853451719,
  -0.031388006623259586,
  -0.0178416602255213,
  0.005453669632757108,
  0.03643194920971762,
  0.0714741437240266,
  0.10595607271388648,
  0.13501769715785947,
  0.1544019807295177,
  0.16120642401817611,
  0.1544019807295177,
  0.13501769715785947,
  0.10595607271388648,
  0.0714741437240266,
  0.03643194920971762,
  0.005453669632757108,
  -0.0178416602255213,
  -0.031388006623259586,
  -0.03491591853451719,
  -0.029848252908431856,
  -0.018891553934292114,
  -0.005417803990550028,
  0.007232604034199318,
  0.016383751711732026,
  0.02047189461312385,
  0.019230199019771778,
  0.013593347998558365,
  0.005364854734696744,
  -0.0032661256223170043,
  -0.010246397991601682,
  -0.014103485003942545,
  -0.014229780437888464,
  -0.010924703994865847,
  -0.005271185205391984,
  0.0011941417654284029,
  0.006866325074436295,
  0.01045043513411174,
  0.011241980882152548,
  0.009249633156439159,
  0.005143955837753482,
  0.00005455829982133947,
  -0.004727109371179884,
  -0.008064398618407978,
  -0.00923726202694145,
  -0.008088227364142088,
  -0.0050288277265914194,
  -0.0009083430915649766,
  0.003214358509674901,
  0.006335564079358473,
  0.007746341353537545,
  0.007188042710323042,
  0.004892917783243419,
  0.0015045074968363624,
  -0.0020976627393834687,
  -0.005022223773762522,
  -0.006583138132634261,
  -0.00645563243609975,
  -0.004739988345774312,
  -0.0019376490537534373,
  0.0012448820029745708,
  0.003987537793798602,
  0.00563788548407116,
  0.005832464615232373,
  0.004576752685339087,
  0.0022298500718601275,
  -0.000592938956035571,
  -0.003177852260521262,
  -0.004892162222141496,
  -0.005338523546370574,
  -0.004445439642947873,
  -0.0024742024014937133,
  0.00005655106722562884,
  0.002505225803218639,
  0.00426804175421079,
  0.004926514748562088,
  0.004343677948935637,
  0.002693703163639939,
  0.0004105056263627215,
  -0.0019244495252580096,
  -0.003728919286706744,
  -0.004563894233780151,
  -0.004238787550945243,
  -0.0028537845954173893,
  -0.000769666530166805,
  0.0014849095218848305,
  0.0033452774503363006,
  0.004353063177238872,
  0.004262544722478894,
  0.0031038502164729017,
  0.0011704366742285258,
  -0.0010543000028954326,
  -0.0030158513710497044,
  -0.004224827358782935,
  -0.004376950825652909,
  -0.0034279259503435917,
  -0.0016041438833185786,
  0.000652193742215706,
  0.0027887012091385795,
  0.004274810366365638,
  0.004729404355044544,
  0.004014283934301812,
  0.0022726954863922866,
  -0.00010727558975114284,
  -0.0025690086034571235,
  -0.0045215943703137106,
  -0.0054719163046588574,
  -0.005140336866112635,
  -0.0035306816963085242,
  -0.0009376321564057613,
  0.0021087251039519934,
  0.004949593517303934,
  0.006931369436850068,
  0.007538459102064722,
  0.006498265087730188,
  0.0038343528384597028,
  -0.00013647892130397337,
  -0.004866825267235354,
  -0.00968260511176664,
  -0.013906238412038235,
  -0.01697603432198143,
  -0.018536462203899054,
  -0.018485751067983044,
  -0.01697428458565556,
  -0.014359258966292856,
  -0.01113092508719082,
  -0.00783227343492291,
  -0.005002322929556022,
  -0.0031818655616045488,
  0.0037078853622880764
],

/*

FIR filter designed with
http://t-filter.appspot.com

sampling frequency: 44100 Hz

* 0 Hz - 3800 Hz
  gain = 1
  desired ripple = 5 dB
  actual ripple = 4.1374988899164595 dB

* 4000 Hz - 22050 Hz
  gain = 0
  desired attenuation = -40 dB
  actual attenuation = -40.07737600565957 dB

*/

44100: [
  0.00488665474794249,
  -0.0008458957886454677,
  -0.0028739639869722875,
  -0.00604749697522329,
  -0.009975948057112726,
  -0.014069104265132235,
  -0.01761034383100156,
  -0.01988569569013061,
  -0.020337133769284692,
  -0.018702308678420234,
  -0.015101014594525043,
  -0.010042120456884102,
  -0.00434122588123118,
  0.0010392920400362346,
  0.00519325261756307,
  0.007469172376134264,
  0.007617093140241852,
  0.005845395500321362,
  0.002768912522137026,
  -0.0007393941606281713,
  -0.0037596751912034793,
  -0.005546681419208735,
  -0.0057107342124650224,
  -0.004297072762079702,
  -0.0017701854534168267,
  0.0011307726424169674,
  0.0035888227266872674,
  0.004932595532824785,
  0.004818492830871363,
  0.003318187218352651,
  0.0008908180562718912,
  -0.0017492346528783396,
  -0.0038362501356754607,
  -0.004771354957998482,
  -0.004292247008056003,
  -0.002546839273877849,
  -0.00005010097346763101,
  0.0024639091471267737,
  0.004253128943615218,
  0.00478340137344329,
  0.003887904471308344,
  0.0018191609037763133,
  -0.0008210875724316434,
  -0.0032534941427238622,
  -0.004747036982033832,
  -0.004841017833186259,
  -0.0034792820941557502,
  -0.0010410669314613354,
  0.0017675113827760346,
  0.004113281865318442,
  0.005280160430432891,
  0.004885263117029309,
  0.0030030065174624513,
  0.0001559791269039415,
  -0.0028272814590297457,
  -0.005051594280983992,
  -0.005822600433401976,
  -0.004860405383092611,
  -0.0023946910061272606,
  0.0008870033371251169,
  0.004026251912866387,
  0.006068229297936733,
  0.006349456843484085,
  0.004714401503007545,
  0.0015823148721959904,
  -0.002166258330940437,
  -0.0054390624507202005,
  -0.0071982080666558165,
  -0.0068622550671842055,
  -0.004420462901056972,
  -0.0005112425998733534,
  0.0037556915920539616,
  0.00710529825435688,
  0.008472019174890688,
  0.007331133366620072,
  0.003886597771263732,
  -0.0009498793611003088,
  -0.005791902693335126,
  -0.009166250311574566,
  -0.009950513622975568,
  -0.007732735182705404,
  -0.00298109285874246,
  0.0030369793779181004,
  0.008578675943357135,
  0.011915089700723072,
  0.011850702456842594,
  0.008123283991693868,
  0.001555787078870509,
  -0.006092882065684752,
  -0.012585919877907257,
  -0.015810139141416782,
  -0.014440658591272295,
  -0.008396354941162684,
  0.0010044240689287335,
  0.01127530902173555,
  0.01935973157703316,
  0.022456789896584763,
  0.018863194727910326,
  0.008608559468842456,
  -0.006304707290557034,
  -0.0221583550785702,
  -0.03425033496499752,
  -0.03795454785180208,
  -0.02986602702001983,
  -0.008753111933844406,
  0.023926544680117772,
  0.06409540673568571,
  0.1057310559912364,
  0.14201639984183795,
  0.1667160953407668,
  0.17546989012844347,
  0.1667160953407668,
  0.14201639984183795,
  0.1057310559912364,
  0.06409540673568571,
  0.023926544680117772,
  -0.008753111933844406,
  -0.02986602702001983,
  -0.03795454785180208,
  -0.03425033496499752,
  -0.0221583550785702,
  -0.006304707290557034,
  0.008608559468842456,
  0.018863194727910326,
  0.022456789896584763,
  0.01935973157703316,
  0.01127530902173555,
  0.0010044240689287335,
  -0.008396354941162684,
  -0.014440658591272295,
  -0.015810139141416782,
  -0.012585919877907257,
  -0.006092882065684752,
  0.001555787078870509,
  0.008123283991693868,
  0.011850702456842594,
  0.011915089700723072,
  0.008578675943357135,
  0.0030369793779181004,
  -0.00298109285874246,
  -0.007732735182705404,
  -0.009950513622975568,
  -0.009166250311574566,
  -0.005791902693335126,
  -0.0009498793611003088,
  0.003886597771263732,
  0.007331133366620072,
  0.008472019174890688,
  0.00710529825435688,
  0.0037556915920539616,
  -0.0005112425998733534,
  -0.004420462901056972,
  -0.0068622550671842055,
  -0.0071982080666558165,
  -0.0054390624507202005,
  -0.002166258330940437,
  0.0015823148721959904,
  0.004714401503007545,
  0.006349456843484085,
  0.006068229297936733,
  0.004026251912866387,
  0.0008870033371251169,
  -0.0023946910061272606,
  -0.004860405383092611,
  -0.005822600433401976,
  -0.005051594280983992,
  -0.0028272814590297457,
  0.0001559791269039415,
  0.0030030065174624513,
  0.004885263117029309,
  0.005280160430432891,
  0.004113281865318442,
  0.0017675113827760346,
  -0.0010410669314613354,
  -0.0034792820941557502,
  -0.004841017833186259,
  -0.004747036982033832,
  -0.0032534941427238622,
  -0.0008210875724316434,
  0.0018191609037763133,
  0.003887904471308344,
  0.00478340137344329,
  0.004253128943615218,
  0.0024639091471267737,
  -0.00005010097346763101,
  -0.002546839273877849,
  -0.004292247008056003,
  -0.004771354957998482,
  -0.0038362501356754607,
  -0.0017492346528783396,
  0.0008908180562718912,
  0.003318187218352651,
  0.004818492830871363,
  0.004932595532824785,
  0.0035888227266872674,
  0.0011307726424169674,
  -0.0017701854534168267,
  -0.004297072762079702,
  -0.0057107342124650224,
  -0.005546681419208735,
  -0.0037596751912034793,
  -0.0007393941606281713,
  0.002768912522137026,
  0.005845395500321362,
  0.007617093140241852,
  0.007469172376134264,
  0.00519325261756307,
  0.0010392920400362346,
  -0.00434122588123118,
  -0.010042120456884102,
  -0.015101014594525043,
  -0.018702308678420234,
  -0.020337133769284692,
  -0.01988569569013061,
  -0.01761034383100156,
  -0.014069104265132235,
  -0.009975948057112726,
  -0.00604749697522329,
  -0.0028739639869722875,
  -0.0008458957886454677,
  0.00488665474794249
]


};