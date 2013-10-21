$(function () {
    //console.log("Salt before encrypt:" + $.jCryption.$el.data("salt"));
    //var password = $.jCryption.encrypt("XXXX", "trubo");
    //var password2 = $.jCryption.encrypt("XXXX", "trubo");
    //var password = "what is this now that we have in front of the only text that the quick brow fox jumped over the lazy dog etc. and then he did it again and again and again.";
    //var password = "U2FsdGVkX19BcNvzq57NG0m4kPFEWZI2VAqhhgCGSuE=U2FsdGVkX19BcNvzq57NG0m4kPFEWZI2VAqhhgCGSuE=2FsdGVkX19BcNvzq57NG0m4kPFEWZI2VAq";
    var password = "mypassword2013";

    console.log(password);
    //console.log(password2);

    $.jCryption.authenticate(password, "/jCryption.aspx/getPublicKey", "/jCryption.aspx/handshake", function (AESKey) {
        $("#text,#encrypt,#decrypt,#serverChallenge").attr("disabled", false);
        $("#status").html('<span style="font-size: 16px;">Let\'s Rock!</span>');
    }, function () {
        // Authentication failed
        console.log('auth failed');
    });

    function encryptSuccess(response) {
        response = response.d;
        $("#log").prepend("\n").prepend("Server decrypted: " + response.data);
    }

    $("#encrypt").click(function () {
        var encryptedString = $.jCryption.encrypt($("#text").val(), password);
        $("#log").prepend("\n").prepend("----------");
        $("#log").prepend("\n").prepend("String: " + $("#text").val());
        $("#log").prepend("\n").prepend("Encrypted: " + encryptedString);

        var parms = { encryptedStr: encryptedString };
        jQueryCallWrapper('/jCryption.aspx/decrypt', parms, encryptSuccess, function () { }, null);

        //$.ajax({
        //    url: "jCryption.aspx",
        //    dataType: "json",
        //    type: "POST",
        //    data: {
        //        jCryption: encryptedString
        //    },
        //    success: function (response) {
        //        $("#log").prepend("\n").prepend("Server decrypted: " + response.data);
        //    }
        //});

    });

    function serverChallengeSuccess(response) {
        response = response.d;
        $("#log").prepend("\n").prepend("----------");
        $("#log").prepend("\n").prepend("Server original: " + response.unencrypted);
        $("#log").prepend("\n").prepend("Server sent: " + response.encrypted);
        var decryptedString = $.jCryption.decrypt(response.encrypted, password);
        $("#log").prepend("\n").prepend("Decrypted: " + decryptedString);
    }

    $("#serverChallenge").click(function () {
        var parms = [];
        jQueryCallWrapper('/jCryption.aspx/decryptTest', parms, serverChallengeSuccess, function () { }, null);

        //$.ajax({
        //    url: "/jCryption.aspx/decryptTest",
        //    dataType: "json",
        //    type: "POST",
        //    success: function (response) {
        //        response = response.d;
        //        $("#log").prepend("\n").prepend("----------");
        //        $("#log").prepend("\n").prepend("Server original: " + response.unencrypted);
        //        $("#log").prepend("\n").prepend("Server sent: " + response.encrypted);
        //        var decryptedString = $.jCryption.decrypt(response.encrypted, password);
        //        $("#log").prepend("\n").prepend("Decrypted: " + decryptedString);
        //    }
        //});
    });

    $("#decrypt").click(function () {
        var decryptedString = $.jCryption.decrypt($("#text").val(), password);
        $("#log").prepend("\n").prepend("----------");
        $("#log").prepend("\n").prepend("Decrypted: " + decryptedString);
    });

    function encryptAndPostFileSuccess(response){
    }
    
    function encryptAndPostFile(fileData, contentType, fileName) {
        //Followed the instructions here:
        //http://stackoverflow.com/questions/17819820/how-to-get-correct-sha1-hash-of-blob-using-cryptojs
        //to include the following library:
        //lib-typedarrays.js
        //to allow the conversion of an array buffer object to a word array for use with
        //CryptoJS

        var wordArray = CryptoJS.lib.WordArray.create(fileData)
        var encryptedData = $.jCryption.encrypt(wordArray, password);
        
        var parms = { encryptedFileData: encryptedData, conType: contentType, name: fileName };
        //jQueryCallWrapper('/jCryption.aspx', parms, encryptAndPostFileSuccess, function () { }, null);
        $.ajax({
            url: "jCryption.aspx",
            dataType: "json",
            type: "POST",
            data: parms,
            //contentType: false,
            //processData: false,
            success: encryptAndPostFileSuccess
        });

        //var blob = file.slice(block.start, block.end);
        // use formdata to send block content in arraybuffer
        //var fd = new FormData();
        //fd.append("name", fileName);
        //fd.append("type", contentType);
        //fd.append("file", encryptedData);
        //$.ajax({
        //    url: "/jCryption.aspx",
        //    data: fd,
        //    processData: false,
        //    contentType: "multipart/form-data",
        //    type: "POST",
        //    success: function (result) {
        //        if (!result.success) {
        //            alert(result.error);
        //        }
        //        //callback(null, block.index);
        //    }
        //});

        
    }

    function handleFileSelect(evt) {
        var files = evt.target.files; // FileList object

        // Loop through the FileList and render image files as thumbnails.
        for (var i = 0, f; f = files[i]; i++) {
            var reader = new FileReader();

            //reader.onload = function (e) {
            //    encryptAndPostFile(e.target.result,f.type,'test.txt');
            //};

            reader.onload = (function (theFile) {
                return function(e){
                    encryptAndPostFile(e.target.result, theFile.type, theFile.name);
                };
            })(f);


            // Read in the image file as a data URL.
            reader.readAsArrayBuffer(f);
            


            
            //encryptAndPostFile(fileData, contentType, f.name);
        }
    }

    document.getElementById('files').addEventListener('change', handleFileSelect, false);

});