/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *			Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/
	 
using System;
using System.IO;
using System.Xml;
using System.Threading;

namespace Simias
{
	/// <summary>
	/// Configuration class for simias components.
	/// </summary>
	public sealed class Configuration
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(Configuration));

		private static readonly string RootElementTag = "configuration";
		private static readonly string SectionTag = "section";
		private static readonly string SettingTag = "setting";
		private static readonly string NameAttr = "name";
		private static readonly string ValueAttr = "value";
		private static readonly string DefaultSection = "SimiasDefault";
		private static readonly string DefaultFileName = "Simias.config";

		private string storePath;
		private FileStream fs = null;
		private bool modified;
		private XmlDocument doc;

		private static Configuration instance = null;
		
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path">The path to the configuration file.</param>
		private Configuration(string path)
		{
			if (path == null)
			{
				path = DefaultPath;
			}
			else
			{
				path = fixupPath(path);
			}

			this.storePath = path;
			CreateDefaults();
		}

		#region Factory  Methods
		/// <summary>
		/// Gets the instance of the configuration object for this process.
		/// </summary>
		/// <returns>A reference to the configuration object.</returns>
		static public Configuration GetConfiguration()
		{
			lock (typeof(Configuration))
			{
				if (instance == null)
					CreateDefaultConfig(null);
				return instance;
			}
		}

		/// <summary>
		/// Creates the default instance of the configuration.
		/// </summary>
		/// <param name="path">Path to where the data store is.</param>
		/// <returns>A reference to the configuration object.</returns>
		static public Configuration CreateDefaultConfig(string path)
		{
			lock (typeof(Configuration))
			{
				if (instance != null)
				{
					throw(new SimiasException("Configuration already exists."));
				}
				instance = new Configuration(path);
				return instance;
			}
		}

		/// <summary>
		/// Releases the instance of the configuration object for this process.
		/// </summary>
		static public void DisposeDefaultConfig()
		{
			lock (typeof(Configuration))
			{
				if (instance != null)
				{
					instance = null;
				}
			}
		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		public void CreateDefaults()
		{
			lock (typeof(Configuration))
			{
				// If the file does not exist look for defaults.
				if (!File.Exists(ConfigFilePath))
				{
					string bootStrapPath = Path.Combine(SimiasSetup.sysconfdir, DefaultFileName);

					if (File.Exists(bootStrapPath))
					{
                        log.Debug("Initializing \"{0}\" with \"{1}\"...", ConfigFilePath, bootStrapPath);
						File.Copy(bootStrapPath, ConfigFilePath);
					}
					else
					{
						fs = new FileStream(ConfigFilePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None );
                        try
                        {
                            doc = new XmlDocument();
                            doc.AppendChild(doc.CreateElement(RootElementTag));
                            doc.Save(fs);
                        }
                        finally
                        {
							doc = null;
							fs.Close();
                        }
					}
				}
			}
		}

		/// <summary>
		/// Called to get the path where simias is installed.
		/// </summary>
		public string StorePath
		{
			get { return storePath; }
		}

		private string ConfigFilePath
		{
			get { return Path.Combine(storePath, DefaultFileName); }
		}

		/// <summary>
		/// Returns the XmlElement for the specified key.  
		/// Creates the key if does not exist.
		/// </summary>
		/// <param name="key">The key to return.</param>
		/// <returns>The key as an XmlElement.</returns>
		public XmlElement GetElement(string key)
		{
			return GetElement(DefaultSection, key);
		}

		/// <summary>
		/// Returns the XmlElement for the specified key.  
		/// Creates the key if does not exist.
		/// </summary>
		/// <param name="section">The section where the key is stored.</param>
		/// <param name="key">The key to return.</param>
		/// <returns>The key as an XmlElement.</returns>
		public XmlElement GetElement(string section, string key)
		{
			XmlElement keyElement = null;
			LoadConfigDocument();

			try
			{
				keyElement = GetKey(section, key);
			}
			finally
			{
				ReleaseConfigDocument();
			}

			return keyElement;
		}

		/// <summary>
		/// Sets the modified element.  The element must have been retrieved from GetElement.
		/// </summary>
		/// <param name="keyElement">The element to save.</param>
		public void SetElement(string section, string key, XmlElement newElement)
		{
			LoadConfigDocument();

			try
			{
				XmlElement keyElement = GetKey(section, key);
				keyElement.InnerXml = newElement.InnerXml;
				modified = true;
			}
			finally
			{
				ReleaseConfigDocument();
			}
		}

		/// <summary>
		/// Returns the value for the specified key.
		/// </summary>
		/// <param name="key">The key to get the value for.</param>
		/// <param name="defaultValue">The default value if no value exists.</param>
		/// <returns>The value as a string.</returns>
		public string Get(string key, string defaultValue)
		{
			return Get(DefaultSection, key, defaultValue);
		}

		/// <summary>
		/// Returns the value for the specified key.
		/// </summary>
		/// <param name="section">The section where the key exists.</param>
		/// <param name="key">The key to get the value for.</param>
		/// <param name="defaultValue">The default value if no value exists.</param>
		/// <returns>The value as a string.</returns>
		public string Get(string section, string key, string defaultValue)
		{
			string keyValue = null;
			LoadConfigDocument();

			try
			{
				XmlElement keyElement = GetKey(section, key);
				keyValue = keyElement.GetAttribute(ValueAttr);
				if (keyValue == "")
				{
					if (defaultValue != null )
					{
						keyElement.SetAttribute(ValueAttr, defaultValue);
						keyValue = defaultValue;
						modified = true;
					}
					else
					{
						keyValue = null;
					}
				}
			}
			finally
			{
				ReleaseConfigDocument();
			}

			return keyValue;
		}

		/// <summary>
		/// Set a Key and value pair.
		/// </summary>
		/// <param name="key">The key to set.</param>
		/// <param name="keyValue">The value of the key.</param>
		public void Set(string key, string keyValue)
		{
			Set(DefaultSection, key, keyValue);
		}

		/// <summary>
		/// Set a key and value pair.
		/// </summary>
		/// <param name="section">The section for the tuple</param>
		/// <param name="key">The key to set.</param>
		/// <param name="keyValue">The value of the key.</param>
		public void Set(string section, string key, string keyValue)
		{
			LoadConfigDocument();

			try
			{
				XmlElement keyElement = GetKey(section, key);
				keyElement.SetAttribute(ValueAttr, keyValue);
				modified = true;
			}
			finally
			{
				ReleaseConfigDocument();
			}
		}

		/// <summary>
		/// Checks for existence of a specified key.
		/// </summary>
		/// <param name="key">The key to check for existence.</param>
		/// <returns>True if the key exists, otherwise false is returned.</returns>
		public bool Exists( string key )
		{
			return Exists( DefaultSection, key );
		}

		/// <summary>
		/// Checks for existence of a specified section and key.
		/// </summary>
		/// <param name="section">The section for the tuple</param>
		/// <param name="key">The key to set.</param>
		/// <returns>True if the section and key exists, otherwise false is returned.</returns>
		public bool Exists( string section, string key )
		{
			LoadConfigDocument();
			bool exists;

			try
			{
				exists = KeyExists(section, key);
			}
			finally
			{
				ReleaseConfigDocument();
			}

			return exists;
		}

		// These two methods are going to read the XML document every
		// time they are called but it's a cheap way to have fresh data
		// and this is probably not called all the time
		private XmlElement GetSection(string section)
		{
			XmlElement sectionElement;

			string str = string.Format("//section[@name='{0}']", section);
			sectionElement = (XmlElement)doc.DocumentElement.SelectSingleNode(str);

			if(sectionElement == null)
			{
				// Create the Section node
				sectionElement = doc.CreateElement(SectionTag);
				sectionElement.SetAttribute(NameAttr, section);
				doc.DocumentElement.AppendChild(sectionElement);
				modified = true;
			}

			return sectionElement;
		}

		private XmlElement GetKey(string section, string key)
		{
			XmlElement keyElement = null;
			XmlElement sectionElement = GetSection(section);

			string xpath = string.Format("//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key);
			keyElement = (XmlElement)sectionElement.SelectSingleNode(xpath);
			if (keyElement == null)
			{				
				keyElement = (XmlElement)sectionElement.OwnerDocument.CreateNode(XmlNodeType.Element, SettingTag, "");
				keyElement.SetAttribute(NameAttr, key);
				sectionElement.AppendChild(keyElement);
				modified = true;
			}

			return keyElement;
		}

		private bool KeyExists(string section, string key)
		{
			bool foundKey = false;
		
			string xpath = string.Format("//{0}[@{1}='{2}']", SectionTag, NameAttr, section);
			XmlElement sectionElement = (XmlElement)doc.DocumentElement.SelectSingleNode(xpath);
			if(sectionElement != null)
			{
				xpath = string.Format("//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key);
				if(sectionElement.SelectSingleNode(xpath) != null)
				{
					foundKey = true;
				}
			}

			return foundKey;
		}

		private void LoadConfigDocument()
		{
			// Open the configuration file.
			OpenConfigFile();

			try
			{
				modified = false;
				doc = new XmlDocument();
				doc.Load(fs);
			}
			catch(Exception e)
			{
				doc = null;
				fs.Close();
				throw new SimiasException("Failed to load config document.", e);
			}
		}

		/// <summary>
		/// Opens the configuration file, retrying if the file is currently in use.
		/// </summary>
		/// <returns>A FileStream object associated with the configuration file.</returns>
		private void OpenConfigFile()
		{
			FileStream fsLocal = null;

			lock (typeof(Configuration))
			{
				// Stay in the loop until the file is opened.
				while ( fsLocal == null )
				{
					try
					{
						// Open the configuration file.
						fsLocal = new FileStream( ConfigFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None );
					}
					catch ( IOException )
					{
						// Wait for a moment before trying to open the file again.
						Thread.Sleep( 100 );
					}
					catch ( Exception e )
					{
						log.Error(e, "Unhandled Exception");

						throw new Exception("Unhandled Exception", e);
					}
				}

				fs = fsLocal;
			}
		}

		private void ReleaseConfigDocument()
		{
			if (modified)
			{
				fs.Position = 0;
				doc.Save(fs);
                fs.SetLength(fs.Position);
				modified = false;
			}

			doc = null;

			// No member variables should be touched after the close.
			fs.Close();
		}

		#region Static Methods
		
		private static string DefaultPath
		{
			get
			{
				string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				if (path == null || path.Length == 0)
				{
					path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				}
				path = fixupPath(path);
				return (path);
			}
		}

		private static string fixupPath(string path)
		{
			if ((path.EndsWith("simias") == false) &&
				(path.EndsWith("simias/") == false) &&
				(path.EndsWith(@"simias\") == false))
			{
				path = Path.Combine(path, "simias");
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			return path;
		}
		#endregion
	}
}


