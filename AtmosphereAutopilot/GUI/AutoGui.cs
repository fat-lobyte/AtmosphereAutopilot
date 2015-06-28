﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace AtmosphereAutopilot
{
	/// <summary>
	/// Attribute for auto-rendered parameters. Use it on property or field to draw it
	/// by AutoGUI.AutoDrawObject method. Supports all basic types and IEnumarable.
	/// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class AutoGuiAttr : Attribute
    {
        internal string value_name;
		internal bool editable;
		internal string format;

		/// <summary>
		/// Set this property or field as auto-renderable.
		/// </summary>
		/// <param name="value_name">Displayed element name</param>
		/// <param name="editable">Can be edited by user. Use for constants only!</param>
		/// <param name="format">If type provides ToString(string format) method, this format string
		/// will be used. You can set it to null if not required</param>
		public AutoGuiAttr(string value_name, bool editable, string format = null)
        {
            this.value_name = value_name;
            this.editable = editable;
            this.format = format;
        }
    }


	/// <summary>
	/// Interface for all windows.
	/// </summary>
    public interface IWindow
    {
		/// <summary>
		/// OnGUI Unity event handler
		/// </summary>
        void OnGUI();

		/// <summary>
		/// Returns true if window is shown.
		/// </summary>
        bool IsShown();

		/// <summary>
		/// Toggle window shown\unshown state
		/// </summary>
        bool ToggleGUI();

		/// <summary>
		/// Hide window. Use for F2 event.
		/// </summary>
        void HideGUI();

		/// <summary>
		/// Unhide window. Use for F2 event.
		/// </summary>
        void UnHideGUI();

		/// <summary>
		/// Show window.
		/// </summary>
        void ShowGUI();

		/// <summary>
		/// Do not show window.
		/// </summary>
        void UnShowGUI();
    }


    /// <summary>
    /// Basic window, derived class needs to implement _drawGUI method.
    /// </summary>
    public abstract class GUIWindow : IWindow
    {
        string wndname;
        int wnd_id;
        bool gui_shown = false;
        bool gui_hidden = false;
        protected Rect window;

		/// <summary>
		/// Create window instance.
		/// </summary>
		/// <param name="wndname">Window header</param>
		/// <param name="wnd_id">Unique for Unity engine id</param>
		/// <param name="window">Initial window position rectangle</param>
        internal GUIWindow(string wndname, int wnd_id, Rect window)
        {
            this.wndname = wndname;
            this.wnd_id = wnd_id;
            this.window = window;
        }

		/// <summary>
		/// Get window header.
		/// </summary>
        public string WindowName { get { return wndname; } }

		/// <inheritdoc />
        public bool IsShown()
        {
            return gui_shown;
        }

		/// <inheritdoc />
        public void OnGUI()
        {
            if (!gui_shown || gui_hidden)
                return;
            GUIStyles.set_colors();
            window = GUILayout.Window(wnd_id, window, _drawGUI, wndname);
            OnGUICustom();
            GUIStyles.reset_colors();
        }

		/// <summary>
		/// Called after each _drawGUI call
		/// </summary>
        protected virtual void OnGUICustom() { }

		/// <inheritdoc />
        public bool ToggleGUI()
        {
            return gui_shown = !gui_shown;
        }

		/// <summary>
		/// Main drawing function
		/// </summary>
		/// <param name="id">Unique window id. Just ignore it in function realization.</param>
        protected abstract void _drawGUI(int id);

		/// <inheritdoc />
        public void HideGUI()
        {
            gui_hidden = true;
        }

		/// <inheritdoc />
        public void UnHideGUI()
        {
            gui_hidden = false;
        }

		/// <inheritdoc />
        public void ShowGUI()
        {
            gui_shown = true;
        }

		/// <inheritdoc />
        public void UnShowGUI()
        {
            gui_shown = false;
        }
    }


	/// <summary>
	/// Automatic property and field rendering functionality
	/// </summary>
    public static class AutoGUI
    {
		// collection of string representations for field values
        static Dictionary<int, string> value_holders = new Dictionary<int, string>();

		// optimization structures
		static Dictionary<Type, PropertyInfo[]> property_list = new Dictionary<Type, PropertyInfo[]>();
		static Dictionary<Type, FieldInfo[]> field_list = new Dictionary<Type, FieldInfo[]>();
		static Dictionary<Type, MethodInfo> toStringMethods = new Dictionary<Type, MethodInfo>();
		static Dictionary<Type, MethodInfo> parseMethods = new Dictionary<Type, MethodInfo>();
		static readonly Type[] formatStrTypes = { typeof(string) };

		/// <summary>
		/// Render class instace using AutoGuiAttr markup.
		/// </summary>
		/// <param name="obj">object to render to current GUILayout.</param>
        public static void AutoDrawObject(object obj)
        {
            Type type = obj.GetType();

            if (type.IsPrimitive)
            {
                draw_primitive(obj);
                return;
            }
			
			// properties
			if (!property_list.ContainsKey(type))
				property_list[type] = type.GetProperties(BindingFlags.Instance | 
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			foreach (var property in property_list[type])
				draw_element(property, obj);

			// fields
			if (!field_list.ContainsKey(type))
				field_list[type] = type.GetFields(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			foreach (var field in field_list[type])
                draw_element(field, obj);
        }


		#region FieldPropertyUniversal

		static object[] GetCustomAttributes(object element, Type atttype, bool inherit)
        {
            PropertyInfo p = element as PropertyInfo;
            if (p != null)
                return p.GetCustomAttributes(atttype, inherit);
            FieldInfo f = element as FieldInfo;
            if (f != null)
                return f.GetCustomAttributes(atttype, inherit);
            return null;
        }

        static Type ElementType(object element)
        {
            PropertyInfo p = element as PropertyInfo;
            if (p != null)
                return p.PropertyType;
            FieldInfo f = element as FieldInfo;
            if (f != null)
                return f.FieldType;
            return null;
        }

        static object GetValue(object element, object obj)
        {
            PropertyInfo p = element as PropertyInfo;
            if (p != null)
                return p.GetValue(obj, null);
            FieldInfo f = element as FieldInfo;
            if (f != null)
                return f.GetValue(obj);
            return null;
        }

        static void SetValue(object element, object obj, object value)
        {
            PropertyInfo p = element as PropertyInfo;
            if (p != null)
                p.SetValue(obj, value, null);
            FieldInfo f = element as FieldInfo;
            if (f != null)
                f.SetValue(obj, value);
        }

        static string Name(object element)
        {
            PropertyInfo p = element as PropertyInfo;
            if (p != null)
                return p.Name;
            FieldInfo f = element as FieldInfo;
            if (f != null)
                return f.Name;
            return null;
        }

		#endregion


		static void draw_primitive(object obj)
        {
            GUILayout.Label(obj.ToString(), GUIStyles.labelStyleRight);
        }

		/// <summary>
		/// Main rendering function.
		/// </summary>
		/// <param name="element">Field or property info to render</param>
		/// <param name="obj">Object instance</param>
        static void draw_element(object element, object obj)
        {
            var attributes = GetCustomAttributes(element, typeof(AutoGuiAttr), true);
            if (attributes == null || attributes.Length <= 0)
                return;
            var att = attributes[0] as AutoGuiAttr;
            if (att == null)
                return;
            Type element_type = ElementType(element);
            if (element_type == null)
                return;

			// If element is collection
            if (typeof(IEnumerable).IsAssignableFrom(element_type))
            {
                IEnumerable list = GetValue(element, obj) as IEnumerable;
                if (list != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(att.value_name + ':', GUIStyles.labelStyleLeft);
                    GUILayout.BeginVertical();
                    foreach (object lel in list)
                        AutoDrawObject(lel);		// render each member
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    return;
                }
            }

            if (element_type == typeof(bool) && att.editable)
            {
                // it's a toggle button
                bool cur_state = (bool)GetValue(element, obj);
                SetValue(element, obj, GUILayout.Toggle(cur_state, att.value_name,
                        GUIStyles.toggleButtonStyle));
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(att.value_name, GUIStyles.labelStyleLeft);

			if (!toStringMethods.ContainsKey(element_type))
				toStringMethods[element_type] = element_type.GetMethod("ToString", formatStrTypes);
			MethodInfo ToStringFormat = toStringMethods[element_type];
            if (!att.editable)
            {
                if (ToStringFormat != null && att.format != null)
                    GUILayout.Label((string)ToStringFormat.Invoke(GetValue(element, obj), new[] { att.format }), GUIStyles.labelStyleRight);
                else
                    GUILayout.Label(GetValue(element, obj).ToString(), GUIStyles.labelStyleRight);
            }
            else
            {
                int hash = 7 * obj.GetHashCode() + 13 * Name(element).GetHashCode();
                string val_holder;
                if (value_holders.ContainsKey(hash))
                    val_holder = value_holders[hash];
                else
                    if (ToStringFormat != null && att.format != null)
                        val_holder = (string)ToStringFormat.Invoke(GetValue(element, obj), new[] { att.format });
                    else
                        val_holder = GetValue(element, obj).ToString();
                val_holder = GUILayout.TextField(val_holder, GUIStyles.textBoxStyle);
                try
                {
					if (!parseMethods.ContainsKey(element_type))
						parseMethods[element_type] = element_type.GetMethod("Parse", formatStrTypes);
					var ParseMethod = parseMethods[element_type];
                    if (ParseMethod != null)
                        SetValue(element, obj, ParseMethod.Invoke(null, new[] { val_holder }));
                }
                catch { }
                value_holders[hash] = val_holder;
            }
            GUILayout.EndHorizontal();
        }
    }
}
