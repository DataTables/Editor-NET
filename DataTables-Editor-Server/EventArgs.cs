using System;
using System.Collections.Generic;

namespace DataTables
{
    /// <summary>
    /// Arguments for the 'PreGet' Editor event
    /// </summary>
    public class PreGetEventArgs : EventArgs
    {
        /// <summary>
        /// Editor instance that triggered the event
        /// </summary>
        public Editor Editor;

        /// <summary>
        /// Id of the row to select - can be null to indicate that all
        /// rows will be selected
        /// </summary>
        public object Id;

        /// <summary>
        /// Event cancel flag. Set to `true` in your event handler to stop any further
        /// processing after the event has been triggered.
        /// </summary>
        public bool Cancel = false;
    }


    /// <summary>
    /// Arguments for the 'PostGet' Editor event
    /// </summary>
    public class PostGetEventArgs : EventArgs
    {
        /// <summary>
        /// Editor instance that triggered the event
        /// </summary>
        public Editor Editor;

        /// <summary>
        /// Id of the row to select - can be null to indicate that all
        /// rows will be selected
        /// </summary>
        public object Id;
  
        /// <summary>
        /// Data read from the database
        /// </summary>
        public List<Dictionary<string, object>> Data;
    }

    /// <summary>
    /// Arguments for the 'PreCreate' Editor event
    /// </summary>
    public class PreCreateEventArgs : EventArgs
    {
        /// <summary>
        /// Editor instance that triggered the event
        /// </summary>
        public Editor Editor;

        /// <summary>
        /// Values submitted to the server by the client
        /// </summary>
        public Dictionary<string, object> Values;

        /// <summary>
        /// Event cancel flag. Set to `true` in your event handler to stop any further
        /// processing after the event has been triggered.
        /// </summary>
        public bool Cancel = false;
    }

    /// <summary>
    /// Arguments for the 'ValidatedCreate' Editor event
    /// </summary>
    public class ValidatedCreateEventArgs : EventArgs
    {
        /// <summary>
        /// Editor instance that triggered the event
        /// </summary>
        public Editor Editor;

        /// <summary>
        /// Values submitted to the server by the client
        /// </summary>
        public Dictionary<string, object> Values;

        /// <summary>
        /// Event cancel flag. Set to `true` in your event handler to stop any further
        /// processing after the event has been triggered.
        /// </summary>
        public bool Cancel = false;
    }


    /// <summary>
    /// Arguments for the 'WriteCreate' Editor event
    /// </summary>
    public class WriteCreateEventArgs : EventArgs
    {
        /// <summary>
        /// Editor instance that triggered the event
        /// </summary>
        public Editor Editor;

        /// <summary>
        /// Newly created row id
        /// </summary>
        public object Id;

        /// <summary>
        /// Data for the new row, submitted by the client
        /// </summary>
        public Dictionary<string, object> Values;
    }


    /// <summary>
    /// Arguments for the 'PostCreate' Editor event
    /// </summary>
    public class PostCreateEventArgs : EventArgs
    {
        /// <summary>
        /// Editor instance that triggered the event
        /// </summary>
        public Editor Editor;

        /// <summary>
        /// Newly created row id
        /// </summary>
        public object Id;

        /// <summary>
        /// Data for the new row, submitted by the client
        /// </summary>
        public Dictionary<string, object> Values;

        /// <summary>
        /// Data for the new row, as read from the database
        /// </summary>
        public Dictionary<string, object> Data;
    }


    /// <summary>
    /// Arguments for the 'WriteEdit' Editor event
    /// </summary>
    public class WriteEditEventArgs : EventArgs
    {
        /// <summary>
        /// Editor instance that triggered the event
        /// </summary>
        public Editor Editor;

        /// <summary>
        /// Edited row ID
        /// </summary>
        public object Id;

        /// <summary>
        /// Data for the row, submitted by the client
        /// </summary>
        public Dictionary<string, object> Values;
    }


    /// <summary>
    /// Arguments for the 'PreEdit' Editor event
    /// </summary>
    public class PreEditEventArgs : EventArgs
    {
        /// <summary>
        /// Editor instance that triggered the event
        /// </summary>
        public Editor Editor;

        /// <summary>
        /// Id of the row to be edited
        /// </summary>
        public object Id;

        /// <summary>
        /// Values submitted to the server by the client
        /// </summary>
        public Dictionary<string, object> Values;

        /// <summary>
        /// Event cancel flag. Set to `true` in your event handler to stop any further
        /// processing after the event has been triggered.
        /// </summary>
        public bool Cancel = false;
    }


    /// <summary>
    /// Arguments for the 'ValidatedEdit' Editor event
    /// </summary>
    public class ValidatedEditEventArgs : EventArgs
    {
        /// <summary>
        /// Editor instance that triggered the event
        /// </summary>
        public Editor Editor;

        /// <summary>
        /// Id of the row to be edited
        /// </summary>
        public object Id;

        /// <summary>
        /// Values submitted to the server by the client
        /// </summary>
        public Dictionary<string, object> Values;

        /// <summary>
        /// Event cancel flag. Set to `true` in your event handler to stop any further
        /// processing after the event has been triggered.
        /// </summary>
        public bool Cancel = false;
    }


    /// <summary>
    /// Arguments for the 'PostEdit' event
    /// </summary>
    public class PostEditEventArgs : EventArgs
    {
        /// <summary>
        /// Editor instance that triggered the event
        /// </summary>
        public Editor Editor;

        /// <summary>
        /// Id of the row to be edited
        /// </summary>
        public object Id;

        /// <summary>
        /// Values submitted to the server by the client
        /// </summary>
        public Dictionary<string, object> Values;

        /// <summary>
        /// Data for the edited row, as read from the database
        /// </summary>
        public Dictionary<string, object> Data;
    }


    /// <summary>
    /// Arguments for the 'PreRemove' Editor event
    /// </summary>
    public class PreRemoveEventArgs : EventArgs
    {
        /// <summary>
        /// Editor instance that triggered the event
        /// </summary>
        public Editor Editor;

        /// <summary>
        /// Id of the row to be removed
        /// </summary>
        public object Id;

        /// <summary>
        /// Values submitted to the server by the client
        /// </summary>
        public Dictionary<string, object> Values;

        /// <summary>
        /// Event cancel flag. Set to `true` in your event handler to stop any further
        /// processing after the event has been triggered.
        /// </summary>
        public bool Cancel = false;
    }


    /// <summary>
    /// Arguments for the 'PostRemove' Editor event
    /// </summary>
    public class PostRemoveEventArgs : EventArgs
    {
        /// <summary>
        /// Editor instance that triggered the event
        /// </summary>
        public Editor Editor;

        /// <summary>
        /// Id of the row to be removed
        /// </summary>
        public object Id;

        /// <summary>
        /// Values submitted to the server by the client
        /// </summary>
        public Dictionary<string, object> Values;
    }


    /// <summary>
    /// Arguments for the 'PreUpload' Editor event
    /// </summary>
    public class PreUploadEventArgs : EventArgs
    {
        /// <summary>
        /// Editor instance that triggered the event
        /// </summary>
        public Editor Editor;

        /// <summary>
        /// Data submitted from the client
        /// </summary>
        public DtRequest Data;

        /// <summary>
        /// Event cancel flag. Set to `true` in your event handler to stop any further
        /// processing after the event has been triggered.
        /// </summary>
        public bool Cancel = false;
    }


    /// <summary>
    /// Arguments for the 'PostUpload' Editor event
    /// </summary>
    public class PostUploadEventArgs : EventArgs
    {
        /// <summary>
        /// Editor instance that triggered the event
        /// </summary>
        public Editor Editor;

        /// <summary>
        /// Id of the file record created
        /// </summary>
        public object Id;

        /// <summary>
        /// Data read from the database about the file uploaded
        /// </summary>
        public Dictionary<string, Dictionary<string, Dictionary<string, object>>> Files;

        /// <summary>
        /// Data submitted from the client
        /// </summary>
        public DtRequest Data;
    }
}
