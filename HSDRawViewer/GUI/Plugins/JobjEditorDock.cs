﻿using HSDRaw.Common;
using HSDRaw.Common.Animation;
using HSDRaw.Melee.Pl;
using HSDRawViewer.Rendering;
using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace HSDRawViewer.GUI.Plugins
{
    [SupportedTypes(new Type[] { typeof(HSD_JOBJ) })]
    public partial class JobjEditorDock : PluginBase
    {
        public override DataNode Node
        {
            get => _node;
            set
            {
                _node = value;

                if (_node.Accessor is HSD_JOBJ jobj)
                    Editor.SetJOBJ(jobj);
            }
        }
        private DataNode _node;

        public JObjEditor Editor { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public JobjEditorDock()
        {
            InitializeComponent();

            Editor = new JObjEditor();
            Editor.Dock = DockStyle.Fill;
            Controls.Add(Editor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="joint"></param>
        public void LoadPhysics(SBM_PhysicsGroup group)
        {
            Editor.LoadPhysics(group);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="joint"></param>
        public void LoadAnimation(HSD_MatAnimJoint joint)
        {
            Editor.LoadAnimation(joint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="anim"></param>
        public void LoadAnimation(JointAnimManager anim)
        {
            Editor.LoadAnimation(anim);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="anim"></param>
        public void LoadAnimation(HSD_ShapeAnimJoint anim)
        {
            Editor.LoadAnimation(anim);
        }
    }
}
