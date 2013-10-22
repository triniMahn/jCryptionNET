<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="jCryptionNET.Default" ValidateRequest="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <p id="status"><span style="font-size: 16px;">Encrypting channel ...</span></p>
    String:
        <input type="text" id="text" disabled="disabled" />
    <button id="encrypt" disabled="disabled">encrypt</button>
    <button id="decrypt" disabled="disabled">decrypt</button>
    <button id="serverChallenge" disabled="disabled">get encrypted time from server</button><br />
        
    Log:<br />

    <textarea cols="60" rows="25" id="log"></textarea>
    <script type="text/javascript" src="//ajax.googleapis.com/ajax/libs/jquery/2.0.3/jquery.min.js"></script>
    <script type="text/javascript" src="/js/jquery-ajax-helpers.js"></script>
    <script type="text/javascript" src="/js/lib/jquery.jcryption.3.0.js"></script>
    <script type="text/javascript" src="/js/lib/lib-typedarrays.js"></script>
    <script type="text/javascript" src="/js/main.js"></script>
</body>
</html>
