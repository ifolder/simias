﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.573
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by wsdl, Version=1.1.4322.573.
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
[System.Web.Services.WebServiceBindingAttribute(Name="Simias Sync ServiceSoap", Namespace="http://novell.com/simias/sync/")]
public class SimiasSyncService : System.Web.Services.Protocols.SoapHttpClientProtocol {
    
    /// <remarks/>
    public SimiasSyncService() {
        this.Url = "http://localhost/SyncService.asmx";
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/Start", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public SyncNodeStamp[] Start(ref SyncStartInfo si, string user) {
        object[] results = this.Invoke("Start", new object[] {
                    si,
                    user});
        si = ((SyncStartInfo)(results[1]));
        return ((SyncNodeStamp[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginStart(SyncStartInfo si, string user, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Start", new object[] {
                    si,
                    user}, callback, asyncState);
    }
    
    /// <remarks/>
    public SyncNodeStamp[] EndStart(System.IAsyncResult asyncResult, out SyncStartInfo si) {
        object[] results = this.EndInvoke(asyncResult);
        si = ((SyncStartInfo)(results[1]));
        return ((SyncNodeStamp[])(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/Stop", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public void Stop() {
        this.Invoke("Stop", new object[0]);
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginStop(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Stop", new object[0], callback, asyncState);
    }
    
    /// <remarks/>
    public void EndStop(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/GetAllNodeStamps", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public SyncNodeStamp[] GetAllNodeStamps() {
        object[] results = this.Invoke("GetAllNodeStamps", new object[0]);
        return ((SyncNodeStamp[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetAllNodeStamps(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetAllNodeStamps", new object[0], callback, asyncState);
    }
    
    /// <remarks/>
    public SyncNodeStamp[] EndGetAllNodeStamps(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNodeStamp[])(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/Version", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string Version() {
        object[] results = this.Invoke("Version", new object[0]);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginVersion(System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Version", new object[0], callback, asyncState);
    }
    
    /// <remarks/>
    public string EndVersion(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/PutNodes", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public SyncNodeStatus[] PutNodes(SyncNode[] nodes) {
        object[] results = this.Invoke("PutNodes", new object[] {
                    nodes});
        return ((SyncNodeStatus[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginPutNodes(SyncNode[] nodes, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("PutNodes", new object[] {
                    nodes}, callback, asyncState);
    }
    
    /// <remarks/>
    public SyncNodeStatus[] EndPutNodes(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNodeStatus[])(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/GetNodes", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public SyncNode[] GetNodes(string[] nids) {
        object[] results = this.Invoke("GetNodes", new object[] {
                    nids});
        return ((SyncNode[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetNodes(string[] nids, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetNodes", new object[] {
                    nids}, callback, asyncState);
    }
    
    /// <remarks/>
    public SyncNode[] EndGetNodes(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNode[])(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/GetDirs", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public SyncNode[] GetDirs(string[] nids) {
        object[] results = this.Invoke("GetDirs", new object[] {
                    nids});
        return ((SyncNode[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetDirs(string[] nids, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetDirs", new object[] {
                    nids}, callback, asyncState);
    }
    
    /// <remarks/>
    public SyncNode[] EndGetDirs(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNode[])(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/PutDirs", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public SyncNodeStatus[] PutDirs(SyncNode[] nodes) {
        object[] results = this.Invoke("PutDirs", new object[] {
                    nodes});
        return ((SyncNodeStatus[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginPutDirs(SyncNode[] nodes, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("PutDirs", new object[] {
                    nodes}, callback, asyncState);
    }
    
    /// <remarks/>
    public SyncNodeStatus[] EndPutDirs(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNodeStatus[])(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/PutFileNode", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public bool PutFileNode(SyncNode node) {
        object[] results = this.Invoke("PutFileNode", new object[] {
                    node});
        return ((bool)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginPutFileNode(SyncNode node, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("PutFileNode", new object[] {
                    node}, callback, asyncState);
    }
    
    /// <remarks/>
    public bool EndPutFileNode(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((bool)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/GetFileNode", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public SyncNode GetFileNode(string nodeID) {
        object[] results = this.Invoke("GetFileNode", new object[] {
                    nodeID});
        return ((SyncNode)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetFileNode(string nodeID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetFileNode", new object[] {
                    nodeID}, callback, asyncState);
    }
    
    /// <remarks/>
    public SyncNode EndGetFileNode(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNode)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/DeleteNodes", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public SyncNodeStatus[] DeleteNodes(string[] nodeIDs) {
        object[] results = this.Invoke("DeleteNodes", new object[] {
                    nodeIDs});
        return ((SyncNodeStatus[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginDeleteNodes(string[] nodeIDs, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("DeleteNodes", new object[] {
                    nodeIDs}, callback, asyncState);
    }
    
    /// <remarks/>
    public SyncNodeStatus[] EndDeleteNodes(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNodeStatus[])(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/GetHashMap", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public HashData[] GetHashMap(int blockSize) {
        object[] results = this.Invoke("GetHashMap", new object[] {
                    blockSize});
        return ((HashData[])(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetHashMap(int blockSize, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetHashMap", new object[] {
                    blockSize}, callback, asyncState);
    }
    
    /// <remarks/>
    public HashData[] EndGetHashMap(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((HashData[])(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/Write", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public void Write([System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] System.Byte[] buffer, long offset, int count) {
        this.Invoke("Write", new object[] {
                    buffer,
                    offset,
                    count});
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginWrite(System.Byte[] buffer, long offset, int count, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Write", new object[] {
                    buffer,
                    offset,
                    count}, callback, asyncState);
    }
    
    /// <remarks/>
    public void EndWrite(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/Copy", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public void Copy(long oldOffset, long offset, int count) {
        this.Invoke("Copy", new object[] {
                    oldOffset,
                    offset,
                    count});
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginCopy(long oldOffset, long offset, int count, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Copy", new object[] {
                    oldOffset,
                    offset,
                    count}, callback, asyncState);
    }
    
    /// <remarks/>
    public void EndCopy(System.IAsyncResult asyncResult) {
        this.EndInvoke(asyncResult);
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/Read", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public int Read(long offset, int count, [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] out System.Byte[] buffer) {
        object[] results = this.Invoke("Read", new object[] {
                    offset,
                    count});
        buffer = ((System.Byte[])(results[1]));
        return ((int)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginRead(long offset, int count, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Read", new object[] {
                    offset,
                    count}, callback, asyncState);
    }
    
    /// <remarks/>
    public int EndRead(System.IAsyncResult asyncResult, out System.Byte[] buffer) {
        object[] results = this.EndInvoke(asyncResult);
        buffer = ((System.Byte[])(results[1]));
        return ((int)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/sync/CloseFileNode", RequestNamespace="http://novell.com/simias/sync/", ResponseNamespace="http://novell.com/simias/sync/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public SyncNodeStatus CloseFileNode(bool commit) {
        object[] results = this.Invoke("CloseFileNode", new object[] {
                    commit});
        return ((SyncNodeStatus)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginCloseFileNode(bool commit, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("CloseFileNode", new object[] {
                    commit}, callback, asyncState);
    }
    
    /// <remarks/>
    public SyncNodeStatus EndCloseFileNode(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SyncNodeStatus)(results[0]));
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/sync/")]
public class SyncStartInfo {
    
    /// <remarks/>
    public string CollectionID;
    
    /// <remarks/>
    public string Context;
    
    /// <remarks/>
    public bool ChangesOnly;
    
    /// <remarks/>
    public bool ClientHasChanges;
    
    /// <remarks/>
    public SyncColStatus Status;
    
    /// <remarks/>
    public Rights Access;
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/sync/")]
public enum SyncColStatus {
    
    /// <remarks/>
    Success,
    
    /// <remarks/>
    NoWork,
    
    /// <remarks/>
    NotFound,
    
    /// <remarks/>
    Busy,
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/sync/")]
public enum Rights {
    
    /// <remarks/>
    Deny,
    
    /// <remarks/>
    ReadOnly,
    
    /// <remarks/>
    ReadWrite,
    
    /// <remarks/>
    Admin,
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/sync/")]
public class HashData {
    
    /// <remarks/>
    public int BlockNumber;
    
    /// <remarks/>
    public System.UInt32 WeakHash;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")]
    public System.Byte[] StrongHash;
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/sync/")]
public class SyncNodeStatus {
    
    /// <remarks/>
    public string nodeID;
    
    /// <remarks/>
    public SyncStatus status;
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/sync/")]
public enum SyncStatus {
    
    /// <remarks/>
    Success,
    
    /// <remarks/>
    UpdateConflict,
    
    /// <remarks/>
    FileNameConflict,
    
    /// <remarks/>
    ServerFailure,
    
    /// <remarks/>
    InProgess,
    
    /// <remarks/>
    InUse,
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/sync/")]
public class SyncNode {
    
    /// <remarks/>
    public string nodeID;
    
    /// <remarks/>
    public string node;
    
    /// <remarks/>
    public System.UInt64 expectedIncarn;
    
    /// <remarks/>
    public SyncOperation operation;
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/sync/")]
public enum SyncOperation {
    
    /// <remarks/>
    Unknown,
    
    /// <remarks/>
    Create,
    
    /// <remarks/>
    Delete,
    
    /// <remarks/>
    Change,
    
    /// <remarks/>
    Rename,
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/sync/")]
public class SyncNodeStamp {
    
    /// <remarks/>
    public string ID;
    
    /// <remarks/>
    public System.UInt64 Incarnation;
    
    /// <remarks/>
    public string BaseType;
    
    /// <remarks/>
    public SyncOperation Operation;
}
