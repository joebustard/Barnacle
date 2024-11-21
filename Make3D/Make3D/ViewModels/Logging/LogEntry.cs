// **************************************************************************
// *   Copyright (c) 2024 Joe Bustard <barnacle3d@gmailcom>                  *
// *                                                                         *
// *   This file is part of the Barnacle 3D application.                     *
// *                                                                         *
// *   This application is free software. You can redistribute it and/or     *
// *   modify it under the terms of the GNU Library General Public           *
// *   License as published by the Free Software Foundation. Either          *
// *   version 2 of the License, or (at your option) any later version.      *
// *                                                                         *
// *   This application is distributed in the hope that it will be useful,   *
// *   but WITHOUT ANY WARRANTY. Without even the implied warranty of        *
// *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
// *   GNU Library General Public License for more details.                  *
// *                                                                         *
// *************************************************************************

using System;

namespace Barnacle.ViewModels.Logging
{
    public class LogEntry
    {
        private String _DateStamp;
        private String _Text;

        // Instance constructor
        public LogEntry()
        {
            _Text = "";
            _DateStamp = "";
        }

        // Copy constructor
        public LogEntry(LogEntry it)
        {
            _Text = it.Text;
            _DateStamp = it.DateStamp;
        }

        public String DateStamp
        {
            get { return _DateStamp; }
            set { _DateStamp = value; }
        }

        public String Text
        {
            get { return _Text; }
            set { _Text = value; }
        }
    }
}