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
[System.Web.Services.WebServiceBindingAttribute(Name="Browser ServiceSoap", Namespace="http://novell.com/simias/browser")]
public class BrowserService : System.Web.Services.Protocols.SoapHttpClientProtocol {
    
    /// <remarks/>
    public BrowserService() {
        this.Url = "http://localhost:8086/simias10/SimiasBrowser.asmx";
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/browser/EnumerateCollections", RequestNamespace="http://novell.com/simias/browser", ResponseNamespace="http://novell.com/simias/browser", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public BrowserNode[] EnumerateCollections() {
        object[] results = this.Invoke("EnumerateCollections", new object[0]);
        return ((BrowserNode[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginEnumerateCollections(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("EnumerateCollections", new object[0], callback, asyncState);
    }
    
    /// <remarks/>
    public BrowserNode[] EndEnumerateCollections(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((BrowserNode[])(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/browser/EnumerateNodes", RequestNamespace="http://novell.com/simias/browser", ResponseNamespace="http://novell.com/simias/browser", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public BrowserNode[] EnumerateNodes(string collectionID) {
        object[] results = this.Invoke("EnumerateNodes", new object[] {
                    collectionID});
        return ((BrowserNode[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginEnumerateNodes(string collectionID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("EnumerateNodes", new object[] {
                    collectionID}, callback, asyncState);
    }
    
    /// <remarks/>
    public BrowserNode[] EndEnumerateNodes(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((BrowserNode[])(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/browser/GetCollectionByID", RequestNamespace="http://novell.com/simias/browser", ResponseNamespace="http://novell.com/simias/browser", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public BrowserNode GetCollectionByID(string collectionID) {
        object[] results = this.Invoke("GetCollectionByID", new object[] {
                    collectionID});
        return ((BrowserNode)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetCollectionByID(string collectionID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetCollectionByID", new object[] {
                    collectionID}, callback, asyncState);
    }
    
    /// <remarks/>
    public BrowserNode EndGetCollectionByID(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((BrowserNode)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/browser/GetNodeByID", RequestNamespace="http://novell.com/simias/browser", ResponseNamespace="http://novell.com/simias/browser", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public BrowserNode GetNodeByID(string collectionID, string nodeID) {
        object[] results = this.Invoke("GetNodeByID", new object[] {
                    collectionID,
                    nodeID});
        return ((BrowserNode)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetNodeByID(string collectionID, string nodeID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetNodeByID", new object[] {
                    collectionID,
                    nodeID}, callback, asyncState);
    }
    
    /// <remarks/>
    public BrowserNode EndGetNodeByID(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((BrowserNode)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/browser/ModifyProperty", RequestNamespace="http://novell.com/simias/browser", ResponseNamespace="http://novell.com/simias/browser", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public void ModifyProperty(string collectionID, string nodeID, string propertyName, string propertyType, string oldPropertyValue, string newPropertyValue, System.UInt32 propertyFlags) {
        this.Invoke("ModifyProperty", new object[] {
                    collectionID,
                    nodeID,
                    propertyName,
                    propertyType,
                    oldPropertyValue,
                    newPropertyValue,
                    propertyFlags});
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginModifyProperty(string collectionID, string nodeID, string propertyName, string propertyType, string oldPropertyValue, string newPropertyValue, System.UInt32 propertyFlags, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("ModifyProperty", new object[] {
                    collectionID,
                    nodeID,
                    propertyName,
                    propertyType,
                    oldPropertyValue,
                    newPropertyValue,
                    propertyFlags}, callback, asyncState);
    }
    
    /// <remarks/>
    public void EndModifyProperty(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/browser/AddProperty", RequestNamespace="http://novell.com/simias/browser", ResponseNamespace="http://novell.com/simias/browser", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public void AddProperty(string collectionID, string nodeID, string propertyName, string propertyType, string propertyValue, System.UInt32 propertyFlags) {
        this.Invoke("AddProperty", new object[] {
                    collectionID,
                    nodeID,
                    propertyName,
                    propertyType,
                    propertyValue,
                    propertyFlags});
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginAddProperty(string collectionID, string nodeID, string propertyName, string propertyType, string propertyValue, System.UInt32 propertyFlags, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("AddProperty", new object[] {
                    collectionID,
                    nodeID,
                    propertyName,
                    propertyType,
                    propertyValue,
                    propertyFlags}, callback, asyncState);
    }
    
    /// <remarks/>
    public void EndAddProperty(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/browser/DeleteProperty", RequestNamespace="http://novell.com/simias/browser", ResponseNamespace="http://novell.com/simias/browser", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public void DeleteProperty(string collectionID, string nodeID, string propertyName, string propertyType, string propertyValue) {
        this.Invoke("DeleteProperty", new object[] {
                    collectionID,
                    nodeID,
                    propertyName,
                    propertyType,
                    propertyValue});
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginDeleteProperty(string collectionID, string nodeID, string propertyName, string propertyType, string propertyValue, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("DeleteProperty", new object[] {
                    collectionID,
                    nodeID,
                    propertyName,
                    propertyType,
                    propertyValue}, callback, asyncState);
    }
    
    /// <remarks/>
    public void EndDeleteProperty(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/browser/DeleteCollection", RequestNamespace="http://novell.com/simias/browser", ResponseNamespace="http://novell.com/simias/browser", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public void DeleteCollection(string collectionID) {
        this.Invoke("DeleteCollection", new object[] {
                    collectionID});
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginDeleteCollection(string collectionID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("DeleteCollection", new object[] {
                    collectionID}, callback, asyncState);
    }
    
    /// <remarks/>
    public void EndDeleteCollection(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/browser/DeleteNode", RequestNamespace="http://novell.com/simias/browser", ResponseNamespace="http://novell.com/simias/browser", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public void DeleteNode(string collectionID, string nodeID) {
        this.Invoke("DeleteNode", new object[] {
                    collectionID,
                    nodeID});
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginDeleteNode(string collectionID, string nodeID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("DeleteNode", new object[] {
                    collectionID,
                    nodeID}, callback, asyncState);
    }
    
    /// <remarks/>
    public void EndDeleteNode(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/browser/EnumerateShallowCollections", RequestNamespace="http://novell.com/simias/browser", ResponseNamespace="http://novell.com/simias/browser", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public BrowserShallowNode[] EnumerateShallowCollections() {
        object[] results = this.Invoke("EnumerateShallowCollections", new object[0]);
        return ((BrowserShallowNode[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginEnumerateShallowCollections(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("EnumerateShallowCollections", new object[0], callback, asyncState);
    }
    
    /// <remarks/>
    public BrowserShallowNode[] EndEnumerateShallowCollections(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((BrowserShallowNode[])(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/browser/EnumerateShallowNodes", RequestNamespace="http://novell.com/simias/browser", ResponseNamespace="http://novell.com/simias/browser", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public BrowserShallowNode[] EnumerateShallowNodes(string collectionID) {
        object[] results = this.Invoke("EnumerateShallowNodes", new object[] {
                    collectionID});
        return ((BrowserShallowNode[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginEnumerateShallowNodes(string collectionID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("EnumerateShallowNodes", new object[] {
                    collectionID}, callback, asyncState);
    }
    
    /// <remarks/>
    public BrowserShallowNode[] EndEnumerateShallowNodes(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((BrowserShallowNode[])(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/browser/GetVersion", RequestNamespace="http://novell.com/simias/browser", ResponseNamespace="http://novell.com/simias/browser", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string GetVersion() {
        object[] results = this.Invoke("GetVersion", new object[0]);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetVersion(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetVersion", new object[0], callback, asyncState);
    }
    
    /// <remarks/>
    public string EndGetVersion(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/browser")]
public class BrowserNode {
    
    /// <remarks/>
    public string NodeData;
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/browser")]
public class BrowserShallowNode {
    
    /// <remarks/>
    public string Name;
    
    /// <remarks/>
    public string ID;
    
    /// <remarks/>
    public string Type;
    
    /// <remarks/>
    public string CID;
}
