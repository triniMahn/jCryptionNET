$(function () {

    //var password = $.jCryption.encrypt("XXXX", "trubo");
    //Can use a "static" password as well, but you'll have left a significant chink in your armour.
    //Doing so, however, does allow you to decrypt data at a later date.
    var password = 'mypassword2013'
    
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
        jQueryCallWrapper('/jCryption.aspx/decrypt', parms, encryptSuccess, genAjaxErrorFunc, null);

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
        jQueryCallWrapper('/jCryption.aspx/decryptTest', parms, serverChallengeSuccess, genAjaxErrorFunc, null);
                
    });

    $("#decrypt").click(function () {
        var decryptedString = $.jCryption.decrypt($("#text").val(), password);
        $("#log").prepend("\n").prepend("----------");
        $("#log").prepend("\n").prepend("Decrypted: " + decryptedString);
    });

    

});