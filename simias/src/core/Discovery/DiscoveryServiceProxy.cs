// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 1.1.4322.2032
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

// 
// This source code was auto-generated by Mono Web Services Description Language Utility
//
    
    /// <remarks/>
    [System.Web.Services.WebServiceBinding(Name="DiscoveryServiceSoap", Namespace="http://novell.com/simias/discovery/")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public class DiscoveryService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        public DiscoveryService() {
            this.Url = "http://127.0.0.1:8086/simias10/DiscoveryService.asmx";
        }
        
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/discovery/GetAllCollectionIDsByUser", RequestNamespace="http://novell.com/simias/discovery/", ResponseNamespace="http://novell.com/simias/discovery/", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
        public string[] GetAllCollectionIDsByUser(string UserID) {
            object[] results = this.Invoke("GetAllCollectionIDsByUser", new object[] {
                UserID});
            return ((string[])(results[0]));
        }
        
        public System.IAsyncResult BeginGetAllCollectionIDsByUser(string UserID, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GetAllCollectionIDsByUser", new object[] {
                UserID}, callback, asyncState);
        }
        
        public string[] EndGetAllCollectionIDsByUser(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string[])(results[0]));
        }
        
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/discovery/GetAllCatalogInfoForUser", RequestNamespace="http://novell.com/simias/discovery/", ResponseNamespace="http://novell.com/simias/discovery/", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
        public CatalogInfo[] GetAllCatalogInfoForUser(string UserID) {
            object[] results = this.Invoke("GetAllCatalogInfoForUser", new object[] {
                UserID});
            return ((CatalogInfo[])(results[0]));
        }
        
        public System.IAsyncResult BeginGetAllCatalogInfoForUser(string UserID, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GetAllCatalogInfoForUser", new object[] {
                UserID}, callback, asyncState);
        }
        
        public CatalogInfo[] EndGetAllCatalogInfoForUser(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((CatalogInfo[])(results[0]));
        }
        
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/discovery/GetAllCollectionsByUser", RequestNamespace="http://novell.com/simias/discovery/", ResponseNamespace="http://novell.com/simias/discovery/", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
        public object[] GetAllCollectionsByUser(string UserID) {
            object[] results = this.Invoke("GetAllCollectionsByUser", new object[] {
                UserID});
            return ((object[])(results[0]));
        }
        
        public System.IAsyncResult BeginGetAllCollectionsByUser(string UserID, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GetAllCollectionsByUser", new object[] {
                UserID}, callback, asyncState);
        }
        
        public object[] EndGetAllCollectionsByUser(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((object[])(results[0]));
        }
        
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/discovery/GetAllMembersOfCollection", RequestNamespace="http://novell.com/simias/discovery/", ResponseNamespace="http://novell.com/simias/discovery/", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
        public string[] GetAllMembersOfCollection(string CollectionID) {
            object[] results = this.Invoke("GetAllMembersOfCollection", new object[] {
                CollectionID});
            return ((string[])(results[0]));
        }
        
        public System.IAsyncResult BeginGetAllMembersOfCollection(string CollectionID, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GetAllMembersOfCollection", new object[] {
                CollectionID}, callback, asyncState);
        }
        
        public string[] EndGetAllMembersOfCollection(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string[])(results[0]));
        }
        
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/discovery/RemoveMemberFromCollection", RequestNamespace="http://novell.com/simias/discovery/", ResponseNamespace="http://novell.com/simias/discovery/", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
        public bool RemoveMemberFromCollection(string CollectionID, string UserID) {
            object[] results = this.Invoke("RemoveMemberFromCollection", new object[] {
                CollectionID,
                UserID});
            return ((bool)(results[0]));
        }
        
        public System.IAsyncResult BeginRemoveMemberFromCollection(string CollectionID, string UserID, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("RemoveMemberFromCollection", new object[] {
                CollectionID,
                UserID}, callback, asyncState);
        }
        
        public bool EndRemoveMemberFromCollection(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((bool)(results[0]));
        }
        
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/discovery/GetCollectionInfo", RequestNamespace="http://novell.com/simias/discovery/", ResponseNamespace="http://novell.com/simias/discovery/", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
        public CollectionInfo GetCollectionInfo(string CollectionID, string UserID) {
            object[] results = this.Invoke("GetCollectionInfo", new object[] {
                CollectionID,
                UserID});
            return ((CollectionInfo)(results[0]));
        }
        
        public System.IAsyncResult BeginGetCollectionInfo(string CollectionID, string UserID, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GetCollectionInfo", new object[] {
                CollectionID,
                UserID}, callback, asyncState);
        }
        
        public CollectionInfo EndGetCollectionInfo(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((CollectionInfo)(results[0]));
        }
        
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://novell.com/simias/discovery/GetCollectionDirNodeID", RequestNamespace="http://novell.com/simias/discovery/", ResponseNamespace="http://novell.com/simias/discovery/", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
        public string GetCollectionDirNodeID(string CollectionID) {
            object[] results = this.Invoke("GetCollectionDirNodeID", new object[] {
                CollectionID});
            return ((string)(results[0]));
        }
        
        public System.IAsyncResult BeginGetCollectionDirNodeID(string CollectionID, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("GetCollectionDirNodeID", new object[] {
                CollectionID}, callback, asyncState);
        }
        
        public string EndGetCollectionDirNodeID(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/discovery/")]
    public class CatalogInfo {
        
        /// <remarks/>
        public string CollectionID;
        
        /// <remarks/>
        public string HostID;
        
        /// <remarks/>
        public string[] UserIDs;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://novell.com/simias/discovery/")]
    public class CollectionInfo {
        
        /// <remarks/>
        public string ID;
        
        /// <remarks/>
        public string CollectionID;
        
        /// <remarks/>
        public string Name;
        
        /// <remarks/>
        public string Description;
        
        /// <remarks/>
        public string OwnerID;
        
        /// <remarks/>
        public string OwnerUserName;
        
        /// <remarks/>
        public string OwnerFullName;
        
        /// <remarks/>
        public string DomainID;
        
        /// <remarks/>
        public long Size;
        
        /// <remarks/>
        public System.DateTime Created;
        
        /// <remarks/>
        public System.DateTime LastModified;
        
        /// <remarks/>
        public int MemberCount;
        
        /// <remarks/>
        public string HostID;
        
        /// <remarks/>
        public string DirNodeID;
        
        /// <remarks/>
        public string DirNodeName;
        
        /// <remarks/>
        public string MemberNodeID;
        
        /// <remarks/>
        public string UserRights;
		
	public string encryptionAlgorithm;
    }
