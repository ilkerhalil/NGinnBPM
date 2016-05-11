﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using NGinnBPM.ProcessModel.Data;

namespace NGinnBPM.ProcessModel
{
    [DataContract(Name="Process")]
    public class ProcessDef : IValidate, IHaveMetadata
    {

        //private string _pname;


        [DataMember]
        public string ProcessName { get;set;}
        
        [DataMember]
        public int Version { get; set; }
        
        [DataMember]
        public string PackageName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Comments { get; set; }
        
        [DataMember]
        public TypeSet DataTypes { get; set; }

        [DataMember]
        public CompositeTaskDef Body { get; set; }

        [IgnoreDataMember]
        public PackageDef Package { get; set; }

        public string DefinitionId
        {
            get { return string.IsNullOrEmpty(PackageName) ? string.Format("{0}.{1}", ProcessName, Version) : string.Format("{0}.{1}.{2}", PackageName, ProcessName, Version); }
        }

        [IgnoreDataMember]
        public string ShortDefinitionId
        {
            get { return string.Format("{0}.{1}", ProcessName, Version); } 
        }

        public PlaceDef GetPlace(string id)
        {
            return Body.FindPlace(id);
        }

        public PlaceDef GetRequiredPlace(string id)
        {
            var pl = GetPlace(id);
            if (pl == null) throw new Exception("Place not found: " + id);
            return pl;
        }

        public TaskDef GetRequiredTask(string id)
        {
            if (id == Body.Id) return Body;
            var td = Body.FindTask(id);
            if (td == null) throw new Exception("Task not found: " + id);
            return td;
        }

        #region IValidate Members

        public bool Validate(List<string> problemsFound)
        {
            if (Body == null)
            {
                problemsFound.Add("Error: process body not defined");
                return false;
            }
            return this.Body.Validate(problemsFound);
        }

        #endregion

        public void FinishModelBuild()
        {
            if (Body == null) return;
            Body.Id = this.ProcessName;
            Body.ParentProcess = this;
            Body.Parent = null;
            Body.UpdateParentRefs();
        }

        #region IHaveExtensionProperties Members

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<string, Dictionary<string, object>> ExtensionProperties { get; set; }



        public object GetMetaValue(string xmlns, string name)
        {
            return ExtensionPropertyHelper.GetExtensionProperty(ExtensionProperties, xmlns, name);
        }

        public void SetMetaValue(string xmlns, string name, object value)
        {
            if (ExtensionProperties == null) ExtensionProperties = new Dictionary<string, Dictionary<string, object>>();
            ExtensionPropertyHelper.SetExtensionProperty(ExtensionProperties, xmlns, name, value);
        }

        Dictionary<string, object> IHaveMetadata.GetMetadata(string ns)
        {
            return ExtensionPropertyHelper.GetExtensionProperties(ExtensionProperties, ns);
        }
        #endregion
    }
}
