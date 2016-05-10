// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Microsoft.VisualStudio.OLE.Interop;

namespace Sitecore.Rocks.Shell.Panes
{
    public class ExecArgs
    {
        private readonly uint _CommandId;

        private readonly Guid _GroupId;

        private uint _CommandExecOpt;

        private IntPtr _pvaIn;

        private IntPtr _pvaOut;

        public ExecArgs(Guid groupId, uint commandId)
        {
            _GroupId = groupId;
            _CommandId = commandId;
        }

        public uint CommandExecOpt
        {
            get { return _CommandExecOpt; }

            set { _CommandExecOpt = value; }
        }

        public uint CommandId
        {
            get { return _CommandId; }
        }

        public Guid GroupId
        {
            get { return _GroupId; }
        }

        public IntPtr PvaIn
        {
            get { return _pvaIn; }

            set { _pvaIn = value; }
        }

        public IntPtr PvaOut
        {
            get { return _pvaOut; }

            set { _pvaOut = value; }
        }
    }

    public sealed class QueryStatusArgs
    {
        private readonly Guid _GroupId;

        private uint _CommandCount;

        private OLECMD[] _Commands;

        private IntPtr _pCmdText;

        public QueryStatusArgs(Guid groupId)
        {
            _GroupId = groupId;
        }

        public uint CommandCount
        {
            get { return _CommandCount; }

            set { _CommandCount = value; }
        }

        public OLECMD[] Commands
        {
            get { return _Commands; }

            set { _Commands = value; }
        }

        public uint FirstCommandId
        {
            get { return _Commands[0].cmdID; }
        }

        public uint FirstCommandStatus
        {
            get { return _Commands[0].cmdf; }

            set { _Commands[0].cmdf = value; }
        }

        public Guid GroupId
        {
            get { return _GroupId; }
        }

        public IntPtr PCmdText
        {
            get { return _pCmdText; }

            set { _pCmdText = value; }
        }
    }
}
