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
[System.Web.Services.WebServiceBindingAttribute(Name="Domain ServiceSoap", Namespace="http://novell.com/ifolder/domain")]
public class DomainService : System.Web.Services.Protocols.SoapHttpClientProtocol {
    
    /// <remarks/>
    public DomainService() {
        this.Url = "http://localhost:8086/simias10/DomainService.asmx";
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/ifolder/domain/GetDomainInfo", RequestNamespace="http://novell.com/ifolder/domain", ResponseNamespace="http://novell.com/ifolder/domain", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public DomainInfo GetDomainInfo(string userID) {
        object[] results = this.Invoke("GetDomainInfo", new object[] {
                    userID});
        return ((DomainInfo)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetDomainInfo(string userID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetDomainInfo", new object[] {
                    userID}, callback, asyncState);
    }
    
    /// <remarks/>
    public DomainInfo EndGetDomainInfo(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((DomainInfo)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/ifolder/domain/ProvisionUser", RequestNamespace="http://novell.com/ifolder/domain", ResponseNamespace="http://novell.com/ifolder/domain", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public ProvisionInfo ProvisionUser(string user, string password) {
        object[] results = this.Invoke("ProvisionUser", new object[] {
                    user,
                    password});
        return ((ProvisionInfo)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginProvisionUser(string user, string password, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("ProvisionUser", new object[] {
                    user,
                    password}, callback, asyncState);
    }
    
    /// <remarks/>
    public ProvisionInfo EndProvisionUser(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((ProvisionInfo)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/ifolder/domain/CreateMaster", RequestNamespace="http://novell.com/ifolder/domain", ResponseNamespace="http://novell.com/ifolder/domain", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string CreateMaster(string collectionID, string collectionName, string rootDirID, string rootDirName, string userID, string memberName, string memberID, string memberRights) {
        object[] results = this.Invoke("CreateMaster", new object[] {
                    collectionID,
                    collectionName,
                    rootDirID,
                    rootDirName,
                    userID,
                    memberName,
                    memberID,
                    memberRights});
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginCreateMaster(string collectionID, string collectionName, string rootDirID, string rootDirName, string userID, string memberName, string memberID, string memberRights, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("CreateMaster", new object[] {
                    collectionID,
                    collectionName,
                    rootDirID,
                    rootDirName,
                    userID,
                    memberName,
                    memberID,
                    memberRights}, callback, asyncState);
    }
    
    /// <remarks/>
    public string EndCreateMaster(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/ifolder/domain/RemoveServerCollections", RequestNamespace="http://novell.com/ifolder/domain", ResponseNamespace="http://novell.com/ifolder/domain", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public void RemoveServerCollections(string domainID, string userID) {
        this.Invoke("RemoveServerCollections", new object[] {
                    domainID,
                    userID});
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginRemoveServerCollections(string domainID, string userID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("RemoveServerCollections", new object[] {
                    domainID,
                    userID}, callback, asyncState);
    }
    
    /// <remarks/>
    public void EndRemoveServerCollections(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/ifolder/domain")]
public class DomainInfo {
    
    /// <remarks/>
    public string Name;
    
    /// <remarks/>
    public string Description;
    
    /// <remarks/>
    public string ID;
    
    /// <remarks/>
    public string RosterID;
    
    /// <remarks/>
    public string RosterName;
    
    /// <remarks/>
    public string MemberNodeID;
    
    /// <remarks/>
    public string MemberNodeName;
    
    /// <remarks/>
    public string MemberRights;
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/ifolder/domain")]
public class ProvisionInfo {
    
    /// <remarks/>
    public string UserID;
    
    /// <remarks/>
    public string POBoxID;
    
    /// <remarks/>
    public string POBoxName;
    
    /// <remarks/>
    public string MemberNodeID;
    
    /// <remarks/>
    public string MemberNodeName;
    
    /// <remarks/>
    public string MemberRights;
}
