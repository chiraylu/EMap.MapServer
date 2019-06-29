﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using IMap.MapServer.Ogc.Ows1_1;
using IMap.MapServer.Ogc.Wmts1;

namespace IMap.MapServer.Ogc.Services
{
    public abstract class WmtsService : OgcService, IWmtsService
    {
        public override string Service => "WMTS";
        public override string Version => "1.0.0";
        public virtual Encoding Encoding { get; } = Encoding.UTF8;
        private static RequestMethodType GetRequestMethodType(string href, string value)
        {
            object[] allowedValues = new Ows1_1.ValueType[]
            {
                new Ows1_1.ValueType()
                {
                     Value=value
                }
            };
            DomainType constraint = new DomainType()
            {
                name = "GetEncoding",
                AllowedValues = allowedValues
            };
            DomainType[] constraints = new DomainType[]
            {
                constraint
            };
            RequestMethodType restRequest = new RequestMethodType()
            {
                href = href,
                Constraint = constraints
            };
            return restRequest;
        }
        public static RequestMethodType GetRestRequestMethodType(string href)
        {
            RequestMethodType restRequest = GetRequestMethodType(href, "REST");
            return restRequest;
        }

        public static RequestMethodType GetKvpRequestMethodType(string href)
        {
            RequestMethodType restRequest = GetRequestMethodType(href, "KVP");
            return restRequest;
        }
        public static Operation GetOperation(string href, string operationName)
        {
            RequestMethodType restRequest = GetRestRequestMethodType(href);
            RequestMethodType kvpRequest = GetKvpRequestMethodType($"{href}?");
            RequestMethodType[] requestMethodTypes = new RequestMethodType[]
            {
                restRequest,kvpRequest
            };
            ItemsChoiceType1[] itemsChoiceType1s = new ItemsChoiceType1[]
            {
                 ItemsChoiceType1.Get, ItemsChoiceType1.Get
            };
            HTTP http = new HTTP()
            {
                Items = requestMethodTypes,
                ItemsElementName = itemsChoiceType1s
            };
            DCP DCP = new DCP()
            {
                Item = http
            };
            DCP[] DCPs = new DCP[]
            {
                DCP
            };
            Operation operation = new Operation()
            {
                name = operationName,
                DCP = DCPs
            };
            return operation;
        }
        public virtual Capabilities CreateCapabilities(string href)
        {
            #region ServiceIdentification
            LanguageStringType[] titles = new LanguageStringType[]
            {
                new LanguageStringType()
                {
                    Value="Web Map Tile Service"
                }
            };
            LanguageStringType[] abstracts = new LanguageStringType[]
            {
                new LanguageStringType()
                {
                    Value="Service that contrains the map access interface to some TileMatrixSets"
                }
            };
            LanguageStringType[] keyword1 = new LanguageStringType[]
            {
                new LanguageStringType()
                {
                    Value="tile"
                }
            };
            KeywordsType keywordsType1 = new KeywordsType()
            {
                Keyword = keyword1
            };
            LanguageStringType[] keyword2 = new LanguageStringType[]
            {
                new LanguageStringType()
                {
                    Value="map"
                }
            };
            KeywordsType keywordsType2 = new KeywordsType()
            {
                Keyword = keyword2
            };
            KeywordsType[] keywords = new KeywordsType[]
            {
                keywordsType1,keywordsType2
            };
            CodeType serviceType = new CodeType()
            {
                Value = "OGC WMTS"
            };
            string[] serviceTypeVersion = new string[]
            {
                "1.0.0"
            };
            string fees = "none";
            string[] accessConstraints = new string[]
            {
                "none"
            };
            ServiceIdentification serviceIdentification = new ServiceIdentification()
            {
                Title = titles,
                Abstract = abstracts,
                Keywords = keywords,
                ServiceType = serviceType,
                ServiceTypeVersion = serviceTypeVersion,
                Fees = fees,
                AccessConstraints = accessConstraints
            };
            #endregion

            #region ServiceProvider
            string poroviderName = "IMap.MapServer";
            OnlineResourceType providerSiteType = new OnlineResourceType()
            {
                href = href
            };
            string[] voices = new string[] { "0000-00000000" };
            string[] facsimiles = new string[] { "0001-00000001" };
            TelephoneType phone = new TelephoneType()
            {
                Voice = voices,
                Facsimile = facsimiles
            };
            string[] deliveryPoints = new string[] { "jinjiang" };
            string city = "chengdu";
            string administrativeArea = "sichuan";
            string country = "china";
            string[] electronicMailAddress = new string[] { "123456@test.com" };
            string postalCode = "123456";
            AddressType address = new AddressType()
            {
                DeliveryPoint = deliveryPoints,
                City = city,
                AdministrativeArea = administrativeArea,
                Country = country,
                ElectronicMailAddress = electronicMailAddress,
                PostalCode = postalCode
            };
            ContactType contactInfo = new ContactType()
            {
                Phone = phone,
                Address = address
            };
            string individualName = "lc";
            string positionName = "Senior Software Engineer";
            ResponsiblePartySubsetType serviceContact = new ResponsiblePartySubsetType()
            {
                IndividualName = individualName,
                PositionName = positionName,
                ContactInfo = contactInfo
            };
            ServiceProvider serviceProvider = new ServiceProvider()
            {
                ProviderName = poroviderName,
                ProviderSite = providerSiteType,
                ServiceContact = serviceContact
            };
            #endregion

            #region OperationsMetadata
            Operation getCapabilitiesOperation = GetOperation(href, "GetCapabilities");
            Operation getTileOperation = GetOperation(href, "GetTile");
            Operation getFeatureinfoOperation = GetOperation(href, "GetFeatureinfo");
            Operation[] operations = new Operation[]
            {
                getCapabilitiesOperation,
                getTileOperation,
                getFeatureinfoOperation
            };
            OperationsMetadata operationsMetadata = new OperationsMetadata()
            {
                Operation = operations
            };
            #endregion

            Capabilities capabilities = new Capabilities()
            {
                ServiceIdentification = serviceIdentification,
                ServiceProvider = serviceProvider,
                OperationsMetadata = operationsMetadata
            };
            return capabilities;
        }
        public abstract LayerType AddContent(Capabilities capabilities, string dataPath);
        public virtual void RemoveContent(Capabilities capabilities, string contentIdentifier)
        {
            //remove content
            DatasetDescriptionSummaryBaseType[] srcContents = capabilities?.Contents?.DatasetDescriptionSummary;
            if (srcContents == null || string.IsNullOrWhiteSpace(contentIdentifier))
            {
                return;
            }
            LayerType content = null;
            for (int i = 0; i < srcContents.Length; i++)
            {
                if (srcContents[i].Identifier.Value == contentIdentifier)
                {
                    content = srcContents[i] as LayerType;
                    break;
                }
            }
            if (content == null)
            {
                return;
            }
            DatasetDescriptionSummaryBaseType[] destContents = srcContents.Remove(content);
            capabilities.Contents.DatasetDescriptionSummary = destContents;
            if (content.TileMatrixSetLink == null || content.TileMatrixSetLink.Length == 0)
            {
                return;
            }
            //remove tileMatrixSet
            TileMatrixSetLink tileMatrixSetLink = content.TileMatrixSetLink[0];
            string tileMatrixSetName = tileMatrixSetLink.TileMatrixSet;
            TileMatrixSet[] tileMatrixSets = capabilities.Contents.TileMatrixSet;
            TileMatrixSet tileMatrixSet = null;
            for (int i = 0; i < tileMatrixSets.Length; i++)
            {
                if (tileMatrixSets[i].Identifier.Value == tileMatrixSetName)
                {
                    tileMatrixSet = tileMatrixSets[i];
                    break;
                }
            }
            if (tileMatrixSet == null)
            {
                return;
            }
            bool isReferenced = false;
            for (int i = 0; i < destContents.Length; i++)
            {
                if (destContents[i] is LayerType layerType)
                {
                    if (layerType.TileMatrixSetLink == null || layerType.TileMatrixSetLink.Length == 0)
                    {
                        continue;
                    }
                    if (layerType.TileMatrixSetLink[0].TileMatrixSet == tileMatrixSetName)
                    {
                        isReferenced = true;
                        break;
                    }
                }
            }
            if (!isReferenced)
            {
                capabilities.Contents.TileMatrixSet = capabilities.Contents.TileMatrixSet.Remove(tileMatrixSet);
            }
        }

        public virtual Capabilities GetCapabilities(string path, GetCapabilities getCapabilities)
        {
            Capabilities capabilities = null;
            if (!File.Exists(path) || getCapabilities?.service != Service || getCapabilities.AcceptVersions == null || !getCapabilities.AcceptVersions.Contains(Version))
            {
                return capabilities;
            }
            try
            {
                XmlSerializer serializer = GetXmlSerializer(typeof(Capabilities));
                using (FileStream fs = File.OpenRead(path))
                {
                    capabilities = (Capabilities)serializer.Deserialize(fs);
                    fs.Close();
                }
            }
            catch { }
            return capabilities;
        }

        public abstract FeatureInfoResponse GetFeatureInfo(string path, GetFeatureInfo getFeatureInfo);

        public abstract byte[] GetTile(Capabilities capabilities, string path, GetTile getTile);

        protected XmlAttributeOverrides GetXmlAttributeOverridesForCapabilities()
        {
            XmlAttributeOverrides attrOverrides = new XmlAttributeOverrides();
            XmlAttributes ddsAttrs = new XmlAttributes();
            XmlElementAttribute layerAttr = new XmlElementAttribute
            {
                ElementName = "Layer",
                Type = typeof(LayerType)
            };
            ddsAttrs.XmlElements.Add(layerAttr);
            attrOverrides.Add(typeof(ContentsBaseType), "DatasetDescriptionSummary", ddsAttrs);
            return attrOverrides;
        }
        protected XmlSerializer GetXmlSerializer(Type type)
        {
            XmlAttributeOverrides xmlAttributeOverrides = GetXmlAttributeOverridesForCapabilities();
            XmlSerializer xmlSerializer = new XmlSerializer(type, xmlAttributeOverrides);
            return xmlSerializer;
        }
        public void XmlSerialize(TextWriter textWriter, Capabilities capabilities)
        {
            if (textWriter != null && capabilities != null)
            {
                XmlSerializer serializer = GetXmlSerializer(typeof(Capabilities));
                serializer.Serialize(textWriter, capabilities);
            }
        }

        public Capabilities XmlDeSerialize(TextReader textReader)
        {
            Capabilities capabilities = null;
            if (textReader != null)
            {
                XmlSerializer serializer = GetXmlSerializer(typeof(Capabilities));
                capabilities = (Capabilities)serializer.Deserialize(textReader);
            }
            return capabilities;
        }
    }
}
