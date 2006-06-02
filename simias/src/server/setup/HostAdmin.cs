﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.2032
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by wsdl, Version=1.1.4322.2032.
// 
using System.Diagnostics;
using System.Xml.Serialization;
using System;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Services;


/// <remarks/>
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Web.Services.WebServiceBindingAttribute(Name="HostAdminSoap", Namespace="http://novell.com/simias/host")]
public class HostAdmin : System.Web.Services.Protocols.SoapHttpClientProtocol {
    
    /// <remarks/>
    public HostAdmin() {
        this.Url = "http://localhost:8086/simias10/HostAdmin.asmx";
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/host/SetHomeServer", RequestNamespace="http://novell.com/simias/host", ResponseNamespace="http://novell.com/simias/host", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public bool SetHomeServer(string userID, string serverID) {
        object[] results = this.Invoke("SetHomeServer", new object[] {
                    userID,
                    serverID});
        return ((bool)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginSetHomeServer(string userID, string serverID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("SetHomeServer", new object[] {
                    userID,
                    serverID}, callback, asyncState);
    }
    
    /// <remarks/>
    public bool EndSetHomeServer(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((bool)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/host/MigrateUser", RequestNamespace="http://novell.com/simias/host", ResponseNamespace="http://novell.com/simias/host", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public bool MigrateUser(string userID) {
        object[] results = this.Invoke("MigrateUser", new object[] {
                    userID});
        return ((bool)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginMigrateUser(string userID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("MigrateUser", new object[] {
                    userID}, callback, asyncState);
    }
    
    /// <remarks/>
    public bool EndMigrateUser(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((bool)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/host/ProvisionUser", RequestNamespace="http://novell.com/simias/host", ResponseNamespace="http://novell.com/simias/host", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public bool ProvisionUser() {
        object[] results = this.Invoke("ProvisionUser", new object[0]);
        return ((bool)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginProvisionUser(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("ProvisionUser", new object[0], callback, asyncState);
    }
    
    /// <remarks/>
    public bool EndProvisionUser(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((bool)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/host/AddHost", RequestNamespace="http://novell.com/simias/host", ResponseNamespace="http://novell.com/simias/host", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string AddHost(string name, string publicAddress, string privateAddress, string publicKey, out bool created) {
        object[] results = this.Invoke("AddHost", new object[] {
                    name,
                    publicAddress,
                    privateAddress,
                    publicKey});
        created = ((bool)(results[1]));
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginAddHost(string name, string publicAddress, string privateAddress, string publicKey, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("AddHost", new object[] {
                    name,
                    publicAddress,
                    privateAddress,
                    publicKey}, callback, asyncState);
    }
    
    /// <remarks/>
    public string EndAddHost(System.IAsyncResult asyncResult, out bool created) {
        object[] results = this.EndInvoke(asyncResult);
        created = ((bool)(results[1]));
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/host/DeleteHost", RequestNamespace="http://novell.com/simias/host", ResponseNamespace="http://novell.com/simias/host", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public void DeleteHost(string id) {
        this.Invoke("DeleteHost", new object[] {
                    id});
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginDeleteHost(string id, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("DeleteHost", new object[] {
                    id}, callback, asyncState);
    }
    
    /// <remarks/>
    public void EndDeleteHost(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/host/GetConfiguration", RequestNamespace="http://novell.com/simias/host", ResponseNamespace="http://novell.com/simias/host", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string GetConfiguration() {
        object[] results = this.Invoke("GetConfiguration", new object[0]);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetConfiguration(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetConfiguration", new object[0], callback, asyncState);
    }
    
    /// <remarks/>
    public string EndGetConfiguration(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/host/GetDomain", RequestNamespace="http://novell.com/simias/host", ResponseNamespace="http://novell.com/simias/host", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string GetDomain() {
        object[] results = this.Invoke("GetDomain", new object[0]);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetDomain(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetDomain", new object[0], callback, asyncState);
    }
    
    /// <remarks/>
    public string EndGetDomain(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/host/GetDomainOwner", RequestNamespace="http://novell.com/simias/host", ResponseNamespace="http://novell.com/simias/host", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string GetDomainOwner() {
        object[] results = this.Invoke("GetDomainOwner", new object[0]);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetDomainOwner(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetDomainOwner", new object[0], callback, asyncState);
    }
    
    /// <remarks/>
    public string EndGetDomainOwner(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
}