/**************************
*
* Armorvox Class
* (c) Auraya Systems 2019
*
* Uses API version 6. This is the most efficient for sending audio compared to v8 which encodes as base64.
*
* Example use:

	var av = new Armorvox('https://cloud.armorvox.com/evaluation/v6', 'auraya');
	
	av.enrol('bob','digits',wavs_array)
	.then((result) => {
		// do something on success
	})
	.catch((result) => {
		// display an error
	});

*
*
***************************/

/**
* Creates an Armorvox object used to communicate with an armorvox server 
*
* @constructor
* @param {string} URL to server
* @param {string} Group name for licence
*/
function Armorvox(server, group, phrase_fa=0.1) {
	this.server = server;
	this.group = group;
	this.phrase_fa = phrase_fa;
}


Armorvox.prototype.delete = function(id, print_name, overrides=[]) {
	
	console.log('Armorvox delete: ', id, print_name);
	
	var data = new FormData();
	data.append('group', this.group);
	data.append('id', id);
	data.append('print_name', print_name);
	
	if (overrides.length > 0) {
		data.append('override',  overrides.join('\n'));
	}

	return this.xmlPromise('delete', data);
}

Armorvox.prototype.check_enrolled = function(id, print_name, overrides=[]) {
	
	console.log('Armorvox check_enrolled: ', id, print_name);
	
	var data = new FormData();
	data.append('group', this.group);
	data.append('id', id);
	data.append('print_name', print_name);
	
	if (overrides.length > 0) {
		data.append('override',  overrides.join('\n'));
	}

	return this.xmlPromise('check_enrolled', data);
}

Armorvox.prototype.enrol = function(id, print_name, wavs_array, overrides=[]) {
	
	console.log('Armorvox enrol: ', id, print_name);
	
	var data = new FormData();
	data.append('group', this.group);
	data.append('id', id);
	data.append('print_name', print_name);
	for (var i = 0; i < wavs_array.length; i++) {
		data.append('utterance'+(i+1), wavs_array[i]);
	}
	
	overrides.push('enrol.qa.ubm_fr_prob=0.01');
	
	if (overrides.length > 0) {
		data.append('override',  overrides.join('\n'));
	}
	
	return this.xmlPromise('enrol', data);
}

Armorvox.prototype.check = function(print_name, wav, phrase=null, mode='enrol', overrides=[]) {
	
	console.log('Armorvox check: ', print_name, phrase, mode);
	
	var data = new FormData();
	data.append('group', this.group);
	data.append('print_name', print_name);
	data.append('utterance', wav);
	data.append('mode', mode);
	
	
	if (phrase) {
		data.append('phrase', phrase);
		//data.append('vocab', vocab);
	}
	
	overrides.push('verify.qa.ubm_fr_prob=0.01');
	overrides.push('enrol.qa.ubm_fr_prob=0.01');
	overrides.push('verify.qa.phrase_fa_prob='+this.phrase_fa);
	overrides.push('enrol.qa.phrase_fa_prob='+this.phrase_fa);
	
	if (overrides.length > 0) {
		data.append('override',  overrides.join('\n'));
	}

	return this.xmlPromise('check_quality', data);
}

Armorvox.prototype.verify = function(id, print_name, wav, phrase=null, al_rate=0.0, overrides = []) {
	
	console.log('Armorvox enrol: ', id, print_name, phrase);
	
	var data = new FormData();
	data.append('group', this.group);
	data.append('id', id);
	data.append('print_name', print_name);
	data.append('utterance', wav);
	
	if (phrase != null) {
		data.append('phrase', phrase);
		//data.append('vocab', vocab);
	}
	
	overrides.push('verify.qa.ubm_fr_prob=0.01');
	overrides.push('verify.qa.phrase_fa_prob='+this.phrase_fa);
		
	overrides.push('active_learning.rate='+al_rate);
	overrides.push('active_learning.enabled=true');
	
	
	if (overrides.length > 0) {
		data.append('override',  overrides.join('\n'));
	}
	
	return this.xmlPromise('verify', data);
}



Armorvox.prototype.xmlPromise = function(api, data) {
	return new Promise((resolve, reject) => {
		var xhr = new XMLHttpRequest();
		xhr.open("POST", this.server + '/' + api);
		xhr.onreadystatechange = (e) => {
			
			if (e.target.readyState === XMLHttpRequest.DONE && e.target.status === 200) {
				
				var extra = this.getValue(e.target.responseText, 'extra');
				var response = {condition: this.getValue(e.target.responseText, 'condition'), extra:extra};
				
				extra.split(',').forEach((s) => {
					var parts = s.split(/:(.+)/,2);
					if (parts.length == 2) {
						if (parts[0].startsWith('utterance')) {
							if (typeof(response[parts[0]]) == 'undefined') {
								response[parts[0]]={};
							}
							
							var inner_parts = parts[1].split(/:|(>=|<|>|<=)/);
							var inner_array = inner_parts.slice(1).filter((i) => {return i != null;});
							response[parts[0]][inner_parts[0]] = inner_array;
						} else {
							response[parts[0]]=parts[1];
						}
					}
				});
	
				
				
				resolve(response);
			}
			if (e.target.status !== 200) {
				reject();
			}
		}
		
		xhr.send(data);	
	});
}


// private
Armorvox.prototype.getValue = function(s, name) {
  var oParser = new DOMParser();
  var xml = oParser.parseFromString(s, "text/xml");
  var xpath = '//ns:var[@name="' + name + '"]/@expr';
  var val = xml.evaluate(xpath, xml, (e) => {return 'http://www.w3.org/2001/vxml';}, XPathResult.STRING_TYPE, null);
  val = val.stringValue.substr(1).slice(0, -1);
  console.log(name + "=" + val);
  return val;
}
