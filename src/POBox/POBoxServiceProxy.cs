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
[System.Web.Services.WebServiceBindingAttribute(Name="POBoxServiceSoap", Namespace="http://novell.com/simias/pobox/")]
public class POBoxService : System.Web.Services.Protocols.SoapHttpClientProtocol {
    
    /// <remarks/>
    public POBoxService() {
        this.Url = "http://137.65.57.4:8086/simias10/POBoxService.asmx";
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/Ping", RequestNamespace="http://novell.com/simias/pobox/", ResponseNamespace="http://novell.com/simias/pobox/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public int Ping(int sleepFor) {
        object[] results = this.Invoke("Ping", new object[] {
                    sleepFor});
        return ((int)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginPing(int sleepFor, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Ping", new object[] {
                    sleepFor}, callback, asyncState);
    }
    
    /// <remarks/>
    public int EndPing(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((int)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/AcceptedSubscription", RequestNamespace="http://novell.com/simias/pobox/", ResponseNamespace="http://novell.com/simias/pobox/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public POBoxStatus AcceptedSubscription(string domainID, string fromIdentity, string toIdentity, string subscriptionID) {
        object[] results = this.Invoke("AcceptedSubscription", new object[] {
                    domainID,
                    fromIdentity,
                    toIdentity,
                    subscriptionID});
        return ((POBoxStatus)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginAcceptedSubscription(string domainID, string fromIdentity, string toIdentity, string subscriptionID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("AcceptedSubscription", new object[] {
                    domainID,
                    fromIdentity,
                    toIdentity,
                    subscriptionID}, callback, asyncState);
    }
    
    /// <remarks/>
    public POBoxStatus EndAcceptedSubscription(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((POBoxStatus)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/DeclinedSubscription", RequestNamespace="http://novell.com/simias/pobox/", ResponseNamespace="http://novell.com/simias/pobox/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public POBoxStatus DeclinedSubscription(string domainID, string fromIdentity, string toIdentity, string subscriptionID) {
        object[] results = this.Invoke("DeclinedSubscription", new object[] {
                    domainID,
                    fromIdentity,
                    toIdentity,
                    subscriptionID});
        return ((POBoxStatus)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginDeclinedSubscription(string domainID, string fromIdentity, string toIdentity, string subscriptionID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("DeclinedSubscription", new object[] {
                    domainID,
                    fromIdentity,
                    toIdentity,
                    subscriptionID}, callback, asyncState);
    }
    
    /// <remarks/>
    public POBoxStatus EndDeclinedSubscription(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((POBoxStatus)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/AckSubscription", RequestNamespace="http://novell.com/simias/pobox/", ResponseNamespace="http://novell.com/simias/pobox/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public POBoxStatus AckSubscription(string domainID, string fromIdentity, string toIdentity, string messageID) {
        object[] results = this.Invoke("AckSubscription", new object[] {
                    domainID,
                    fromIdentity,
                    toIdentity,
                    messageID});
        return ((POBoxStatus)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginAckSubscription(string domainID, string fromIdentity, string toIdentity, string messageID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("AckSubscription", new object[] {
                    domainID,
                    fromIdentity,
                    toIdentity,
                    messageID}, callback, asyncState);
    }
    
    /// <remarks/>
    public POBoxStatus EndAckSubscription(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((POBoxStatus)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/GetSubscriptionInfo", RequestNamespace="http://novell.com/simias/pobox/", ResponseNamespace="http://novell.com/simias/pobox/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public SubscriptionInformation GetSubscriptionInfo(string domainID, string identityID, string messageID) {
        object[] results = this.Invoke("GetSubscriptionInfo", new object[] {
                    domainID,
                    identityID,
                    messageID});
        return ((SubscriptionInformation)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetSubscriptionInfo(string domainID, string identityID, string messageID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetSubscriptionInfo", new object[] {
                    domainID,
                    identityID,
                    messageID}, callback, asyncState);
    }
    
    /// <remarks/>
    public SubscriptionInformation EndGetSubscriptionInfo(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((SubscriptionInformation)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/VerifyCollection", RequestNamespace="http://novell.com/simias/pobox/", ResponseNamespace="http://novell.com/simias/pobox/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public POBoxStatus VerifyCollection(string domainID, string collectionID) {
        object[] results = this.Invoke("VerifyCollection", new object[] {
                    domainID,
                    collectionID});
        return ((POBoxStatus)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginVerifyCollection(string domainID, string collectionID, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("VerifyCollection", new object[] {
                    domainID,
                    collectionID}, callback, asyncState);
    }
    
    /// <remarks/>
    public POBoxStatus EndVerifyCollection(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((POBoxStatus)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/Invite", RequestNamespace="http://novell.com/simias/pobox/", ResponseNamespace="http://novell.com/simias/pobox/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string Invite(string domainID, string fromUserID, string toUserID, string sharedCollectionID, string sharedCollectionType, int rights) {
        object[] results = this.Invoke("Invite", new object[] {
                    domainID,
                    fromUserID,
                    toUserID,
                    sharedCollectionID,
                    sharedCollectionType,
                    rights});
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginInvite(string domainID, string fromUserID, string toUserID, string sharedCollectionID, string sharedCollectionType, int rights, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("Invite", new object[] {
                    domainID,
                    fromUserID,
                    toUserID,
                    sharedCollectionID,
                    sharedCollectionType,
                    rights}, callback, asyncState);
    }
    
    /// <remarks/>
    public string EndInvite(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/pobox/GetDefaultDomain", RequestNamespace="http://novell.com/simias/pobox/", ResponseNamespace="http://novell.com/simias/pobox/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    public string GetDefaultDomain(int dummy) {
        object[] results = this.Invoke("GetDefaultDomain", new object[] {
                    dummy});
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginGetDefaultDomain(int dummy, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("GetDefaultDomain", new object[] {
                    dummy}, callback, asyncState);
    }
    
    /// <remarks/>
    public string EndGetDefaultDomain(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((string)(results[0]));
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/pobox/")]
public enum POBoxStatus {
    
    /// <remarks/>
    Success,
    
    /// <remarks/>
    UnknownPOBox,
    
    /// <remarks/>
    UnknownIdentity,
    
    /// <remarks/>
    UnknownSubscription,
    
    /// <remarks/>
    UnknownCollection,
    
    /// <remarks/>
    InvalidState,
    
    /// <remarks/>
    UnknownError,
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/pobox/")]
public class SubscriptionInformation {
    
    /// <remarks/>
    public string Name;
    
    /// <remarks/>
    public string MsgID;
    
    /// <remarks/>
    public string FromID;
    
    /// <remarks/>
    public string FromName;
    
    /// <remarks/>
    public string ToID;
    
    /// <remarks/>
    public string ToNodeID;
    
    /// <remarks/>
    public string ToName;
    
    /// <remarks/>
    public int AccessRights;
    
    /// <remarks/>
    public string CollectionID;
    
    /// <remarks/>
    public string CollectionName;
    
    /// <remarks/>
    public string CollectionType;
    
    /// <remarks/>
    public string CollectionUrl;
    
    /// <remarks/>
    public string DirNodeID;
    
    /// <remarks/>
    public string DirNodeName;
    
    /// <remarks/>
    public string DomainID;
    
    /// <remarks/>
    public string DomainName;
    
    /// <remarks/>
    public int State;
    
    /// <remarks/>
    public int Disposition;
}
