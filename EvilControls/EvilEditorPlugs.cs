﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using EvilBase;

namespace EvilControls
{
    public partial class EvilEditorPlugs : Component
    {
        protected EvilPlugList m_PluginList = null;
        protected Timer m_Timer = new Timer();
        protected List<IPlugEditor> m_Editors = new List<IPlugEditor>();
        protected Form m_Parent = null;
        protected ToolStripMenuItem m_ToolsMenu = null;

        public EvilEditorPlugs()
        {
            InitializeComponent();

            InitLoadTimer();
        }

        private void InitLoadTimer()
        {
            m_Timer.Interval = 100;
            m_Timer.Enabled = true;
            m_Timer.Tick += new EventHandler(m_Timer_Tick);
        }

        void m_Timer_Tick(object sender, EventArgs e)
        {
            m_Timer.Enabled = false;
            m_Timer.Dispose();

            LoadEditors();
        }

        public EvilEditorPlugs(IContainer container)
        {
            InitializeComponent();
            container.Add(this);

            InitLoadTimer();
        }

        private void LoadEditors()
        {
            if ((DesignMode) || (EvilTools.DesignMode))
                return;

            List<Type> tools = m_PluginList.GetPluginTypes(typeof(IPlugEditor));

            foreach (Type t in tools)
            {
                if (t.IsInterface)
                    continue;
                IPlugEditor et = (IPlugEditor)Activator.CreateInstance(t);

                et.Load(m_Parent, m_ToolsMenu);
                et.Enabled = false;
                m_Editors.Add(et);
            }

        }

        public List<IPlugEditor> FindEditorsByExt(string extension)
        {
            if (extension.Substring(0, 1) == ".")
                extension = extension.Substring(1);

            List<IPlugEditor> res = new List<IPlugEditor>();
            foreach (IPlugEditor editor in m_Editors)
            {
                foreach (IPlugFileType ft in editor.HandledFileTypes)
                {
                    if (ft.Extension == extension)
                    {
                        res.Add(editor);
                        break;
                    }
                }
            }

            return res;
        }

        public Form ParentForm
        {
            get
            {
                return m_Parent;
            }
            set
            {
                m_Parent = value;
            }
        }

        public ToolStripMenuItem ToolsMenu
        {
            get
            {
                return m_ToolsMenu;
            }
            set
            {
                m_ToolsMenu = value;
            }
        }

        public EvilPlugList PluginList
        {
            get
            {
                return m_PluginList;
            }
            set
            {
                m_PluginList = value;
            }
        }

        public void DisableAllEditors()
        {
            foreach (IPlugEditor editor in m_Editors)
            {
                editor.Enabled = false;
            }
        }

        public List<IPlugEditor> EditorPlugs
        {
            get
            {
                return m_Editors;
            }
        }

        public void UnloadAllEditors()
        {
            foreach (IPlugEditor editor in m_Editors)
            {
                if (editor.Enabled)
                {
                    editor.Unload();
                }
            }
        }

        public void OnFileOpen(string FileName)
        {
            foreach (IPlugEditor editor in m_Editors)
            {
                if (editor.Enabled)
                {
                    editor.FileOpened(FileName);
                }
            }
        }
    }
}
