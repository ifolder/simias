/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
[System.ComponentModel.DesignerCategoryAttribute("code")]
public class ClientUpdate : System.Web.Services.Protocols.SoapHttpClientProtocol {
    
    public ClientUpdate() {
        this.Url = "http://164.99.101.16/simias10/ClientUpdate.asmx";
    }
    
    /// <remarks>
///Gets the update files associated with the specified version.
///</remarks>
    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/ifolder/web/GetUpdateFiles", RequestNamespace="http://novell.com/ifolder/web/", ResponseNamespace="http://novell.com/ifolder/web/")]
    public string[] GetUpdateFiles() {
        object[] results = this.Invoke("GetUpdateFiles", new object[0]);
        return ((string[])(results[0]));
    }
    
    public System.IAsyncResult BeginGetUpdateFiles(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetUpdateFiles", new object[0], callback, asyncState);
    }
    
    public string[] EndGetUpdateFiles(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string[])(results[0]));
    }
    
    /// <remarks>
///Gets the update files associated with the specified version.
///</remarks>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/ifolder/web/GetUpdateFilesSoapDocMethod", RequestNamespace="http://novell.com/ifolder/web/", ResponseNamespace="http://novell.com/ifolder/web/", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
    public string[] GetUpdateFilesSoapDocMethod() {
        object[] results = this.Invoke("GetUpdateFilesSoapDocMethod", new object[0]);
        return ((string[])(results[0]));
    }
    
    public System.IAsyncResult BeginGetUpdateFilesSoapDocMethod(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetUpdateFilesSoapDocMethod", new object[0], callback, asyncState);
    }
    
    public string[] EndGetUpdateFilesSoapDocMethod(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string[])(results[0]));
    }
    
    /// <remarks>
///Is Client Update Available
///</remarks>
    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/ifolder/web/IsUpdateAvailableActual", RequestNamespace="http://novell.com/ifolder/web/", ResponseNamespace="http://novell.com/ifolder/web/")]
    public string IsUpdateAvailableActual(string platform, string currentVersion) {
        object[] results = this.Invoke("IsUpdateAvailableActual", new object[] {
            platform,
            currentVersion});
        return ((string)(results[0]));
    }
    
    public System.IAsyncResult BeginIsUpdateAvailableActual(string platform, string currentVersion, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("IsUpdateAvailableActual", new object[] {
            platform,
            currentVersion}, callback, asyncState);
    }
    
    public string EndIsUpdateAvailableActual(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
    
    /// <remarks>
///Is Client Update Available
///</remarks>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/ifolder/web/IsUpdateAvailableActualSoapDocMethod", RequestNamespace="http://novell.com/ifolder/web/", ResponseNamespace="http://novell.com/ifolder/web/", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
    public string IsUpdateAvailableActualSoapDocMethod(string platform, string currentVersion) {
        object[] results = this.Invoke("IsUpdateAvailableActualSoapDocMethod", new object[] {
            platform,
            currentVersion});
        return ((string)(results[0]));
    }
    
    public System.IAsyncResult BeginIsUpdateAvailableActualSoapDocMethod(string platform, string currentVersion, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("IsUpdateAvailableActualSoapDocMethod", new object[] {
            platform,
            currentVersion}, callback, asyncState);
    }
    
    public string EndIsUpdateAvailableActualSoapDocMethod(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
    
    /// <remarks>
///Is Client Update Available
///</remarks>
    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/ifolder/web/IsUpdateAvailable", RequestNamespace="http://novell.com/ifolder/web/", ResponseNamespace="http://novell.com/ifolder/web/")]
    public string IsUpdateAvailable(string platform, string currentVersion) {
        object[] results = this.Invoke("IsUpdateAvailable", new object[] {
            platform,
            currentVersion});
        return ((string)(results[0]));
    }
    
    public System.IAsyncResult BeginIsUpdateAvailable(string platform, string currentVersion, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("IsUpdateAvailable", new object[] {
            platform,
            currentVersion}, callback, asyncState);
    }
    
    public string EndIsUpdateAvailable(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
    
    /// <remarks>
///Is Client Update Available
///</remarks>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/ifolder/web/IsUpdateAvailableSoapDocMethod", RequestNamespace="http://novell.com/ifolder/web/", ResponseNamespace="http://novell.com/ifolder/web/", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
    public string IsUpdateAvailableSoapDocMethod(string platform, string currentVersion) {
        object[] results = this.Invoke("IsUpdateAvailableSoapDocMethod", new object[] {
            platform,
            currentVersion});
        return ((string)(results[0]));
    }
    
    public System.IAsyncResult BeginIsUpdateAvailableSoapDocMethod(string platform, string currentVersion, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("IsUpdateAvailableSoapDocMethod", new object[] {
            platform,
            currentVersion}, callback, asyncState);
    }
    
    public string EndIsUpdateAvailableSoapDocMethod(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
    
    /// <remarks>
///Is server older
///</remarks>
    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/ifolder/web/IsServerOlder", RequestNamespace="http://novell.com/ifolder/web/", ResponseNamespace="http://novell.com/ifolder/web/")]
    public bool IsServerOlder(string platform, string currentVersion) {
        object[] results = this.Invoke("IsServerOlder", new object[] {
            platform,
            currentVersion});
        return ((bool)(results[0]));
    }
    
    public System.IAsyncResult BeginIsServerOlder(string platform, string currentVersion, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("IsServerOlder", new object[] {
            platform,
            currentVersion}, callback, asyncState);
    }
    
    public bool EndIsServerOlder(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((bool)(results[0]));
    }
    
    /// <remarks>
///Is server older
///</remarks>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/ifolder/web/IsServerOlderSoapDocMethod", RequestNamespace="http://novell.com/ifolder/web/", ResponseNamespace="http://novell.com/ifolder/web/", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
    public bool IsServerOlderSoapDocMethod(string platform, string currentVersion) {
        object[] results = this.Invoke("IsServerOlderSoapDocMethod", new object[] {
            platform,
            currentVersion});
        return ((bool)(results[0]));
    }
    
    public System.IAsyncResult BeginIsServerOlderSoapDocMethod(string platform, string currentVersion, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("IsServerOlderSoapDocMethod", new object[] {
            platform,
            currentVersion}, callback, asyncState);
    }
    
    public bool EndIsServerOlderSoapDocMethod(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((bool)(results[0]));
    }
    
    /// <remarks>
///Check for Client Updates and compatibility with server
///</remarks>
    [System.Web.Services.Protocols.SoapRpcMethodAttribute("http://novell.com/ifolder/web/CheckForUpdate", RequestNamespace="http://novell.com/ifolder/web/", ResponseNamespace="http://novell.com/ifolder/web/")]
    public StatusCodes CheckForUpdate(string platform, string currentVersion, out string serverVersion) {
        object[] results = this.Invoke("CheckForUpdate", new object[] {
            platform,
            currentVersion});
        serverVersion = ((string)(results[1]));
        return ((StatusCodes)(results[0]));
    }
    
    public System.IAsyncResult BeginCheckForUpdate(string platform, string currentVersion, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("CheckForUpdate", new object[] {
            platform,
            currentVersion}, callback, asyncState);
    }
    
    public StatusCodes EndCheckForUpdate(System.IAsyncResult asyncResult, out string serverVersion) {
        object[] results = this.EndInvoke(asyncResult);
        serverVersion = ((string)(results[1]));
        return ((StatusCodes)(results[0]));
    }
    
    /// <remarks>
///Check for Client Updates and compatibility with server
///</remarks>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/ifolder/web/CheckForUpdateSoapDocMethod", RequestNamespace="http://novell.com/ifolder/web/", ResponseNamespace="http://novell.com/ifolder/web/", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
    public StatusCodes1 CheckForUpdateSoapDocMethod(string platform, string currentVersion, out string serverVersion) {
        object[] results = this.Invoke("CheckForUpdateSoapDocMethod", new object[] {
            platform,
            currentVersion});
        serverVersion = ((string)(results[1]));
        return ((StatusCodes1)(results[0]));
    }
    
    public System.IAsyncResult BeginCheckForUpdateSoapDocMethod(string platform, string currentVersion, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("CheckForUpdateSoapDocMethod", new object[] {
            platform,
            currentVersion}, callback, asyncState);
    }
    
    public StatusCodes1 EndCheckForUpdateSoapDocMethod(System.IAsyncResult asyncResult, out string serverVersion) {
        object[] results = this.EndInvoke(asyncResult);
        serverVersion = ((string)(results[1]));
        return ((StatusCodes1)(results[0]));
    }
}

/// <remarks/>
[System.Xml.Serialization.SoapType(Namespace="http://novell.com/ifolder/web/encodedTypes")]
public enum StatusCodes {
    
    /// <remarks/>
    Success,
    
    /// <remarks/>
    SuccessInGrace,
    
    /// <remarks/>
    InvalidCertificate,
    
    /// <remarks/>
    UnknownUser,
    
    /// <remarks/>
    AmbiguousUser,
    
    /// <remarks/>
    InvalidCredentials,
    
    /// <remarks/>
    InvalidPassword,
    
    /// <remarks/>
    AccountDisabled,
    
    /// <remarks/>
    AccountLockout,
    
    /// <remarks/>
    SimiasLoginDisabled,
    
    /// <remarks/>
    UnknownDomain,
    
    /// <remarks/>
    InternalException,
    
    /// <remarks/>
    MethodNotSupported,
    
    /// <remarks/>
    Timeout,
    
    /// <remarks/>
    OlderVersion,
    
    /// <remarks/>
    ServerOld,
    
    /// <remarks/>
    UpgradeNeeded,
    
    /// <remarks/>
    PassPhraseNotSet,
    
    /// <remarks/>
    PassPhraseInvalid,
    
    /// <remarks/>
    UserAlreadyMoved,
    
    /// <remarks/>
    Unknown,
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute("StatusCodes", Namespace="http://novell.com/simias/web/")]
public enum StatusCodes1 {
    
    /// <remarks/>
    Success,
    
    /// <remarks/>
    SuccessInGrace,
    
    /// <remarks/>
    InvalidCertificate,
    
    /// <remarks/>
    UnknownUser,
    
    /// <remarks/>
    AmbiguousUser,
    
    /// <remarks/>
    InvalidCredentials,
    
    /// <remarks/>
    InvalidPassword,
    
    /// <remarks/>
    AccountDisabled,
    
    /// <remarks/>
    AccountLockout,
    
    /// <remarks/>
    SimiasLoginDisabled,
    
    /// <remarks/>
    UnknownDomain,
    
    /// <remarks/>
    InternalException,
    
    /// <remarks/>
    MethodNotSupported,
    
    /// <remarks/>
    Timeout,
    
    /// <remarks/>
    OlderVersion,
    
    /// <remarks/>
    ServerOld,
    
    /// <remarks/>
    UpgradeNeeded,
    
    /// <remarks/>
    PassPhraseNotSet,
    
    /// <remarks/>
    PassPhraseInvalid,
    
    /// <remarks/>
    UserAlreadyMoved,
    
    /// <remarks/>
    Unknown,
}
