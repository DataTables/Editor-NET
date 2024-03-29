﻿// <copyright>Copyright (c) SpryMedia Ltd - All Rights Reserved</copyright>
//
// <summary>
// Attributes that can be used for properties in Editor models
// </summary>
using System;
using System.Collections.Generic;
using DataTables.EditorUtil;

namespace DataTables
{
    /// <summary>
    /// Define an error message for cases where the data given cannot be
    /// stored in parameter type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditorTypeErrorAttribute : Attribute
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// Constructor for a field attribute defining an error message
        /// </summary>
        /// <param name="msg">Error message</param>
        public EditorTypeErrorAttribute(string msg)
        {
            Msg = msg;
        }
    }

    /// <summary>
    /// Define the HTTP name (used for both the JSON and incoming HTTP values
    /// on form submit) for the field defined by the parameter. The parameter
    /// name is used as the database column name automatically.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditorHttpNameAttribute : Attribute
    {
        /// <summary>
        /// Field's HTTP name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Constructor for a field attribute defining the HTTP name for a property
        /// </summary>
        /// <param name="name">HTTP name</param>
        public EditorHttpNameAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Tell Editor to not use this property from the model as part of the data to
    /// process (fetch from the database, or expect from the client-side)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditorIgnoreAttribute : Attribute
    {
        /// <summary>
        /// Field's ignore flag
        /// </summary>
        public bool Ignore = false;

        /// <summary>
        /// Constructor for a field attribute telling Editor to ignore a property in a model
        /// </summary>
        /// <param name="ign">Ignore flag</param>
        public EditorIgnoreAttribute(bool ign = true)
        {
            Ignore = ign;
        }
    }

    /// <summary>
    /// Get attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditorGetAttribute : Attribute
    {
        /// <summary>
        /// Field's get flag
        /// </summary>
        public bool Get = false;

        /// <summary>
        /// Constructor for a field attribute telling Editor the get state in a modal
        /// </summary>
        /// <param name="get">Get flag</param>
        public EditorGetAttribute(bool get = true)
        {
            Get = get;
        }
    }

    /// <summary>
    /// Set attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditorSetAttribute : Attribute
    {
        /// <summary>
        /// Field's set flag
        /// </summary>
        public Field.SetType Set = Field.SetType.Both;

        /// <summary>
        /// Constructor for a field attribute telling Editor the set state in a modal
        /// </summary>
        /// <param name="set">Set flag</param>
        public EditorSetAttribute(Field.SetType set = Field.SetType.Both)
        {
            Set = set;
        }
    }
}
