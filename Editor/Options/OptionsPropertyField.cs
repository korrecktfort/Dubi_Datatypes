using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class OptionsPropertyField : VisualElement
{
    public new class UxmlFactory : UxmlFactory<OptionsPropertyField, UxmlTraits> { }

    SerializedProperty property = null;    
    Type fieldType = null;

    SerializedObject serializedObject = null;
    IJSON iJSON = null;

    OptionsPropertyFieldBind bind = null;
    Action<SerializedObject> additionalBindingAction = null;

    public OptionsPropertyField()
    {
        AddToClassList("options-property-field");

        VisualElement options = new VisualElement() { name = "Options"};
        options.AddToClassList("options-property-field__options");        
        Add(options);

        PropertyField propertyField = new PropertyField();
        propertyField.AddToClassList("options-property-field__property-field");
        propertyField.RegisterValueChangeCallback(OnPropertyValueChanged);
        Add(propertyField);

        RegisterCallback<AttachToPanelEvent>(OnAttachToPanelEvent);
        RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanelEvent);
    }

    private void OnPropertyValueChanged(SerializedPropertyChangeEvent evt)
    {
        if (evt.changedProperty.objectReferenceValue == null)
            BindSerializedObject(null);
        else
            BindSerializedObject(new SerializedObject(evt.changedProperty.objectReferenceValue));
    }

    public void SetupSerializedObjectBind(OptionsPropertyFieldBind bind)
    {
        this.bind = bind;

        this.bind?.BindSerializedObject(this.serializedObject);        
    }

    public void SetupSerializedObjectBind(Action<SerializedObject> bind)
    {
        this.additionalBindingAction = bind;
    }

    private void OnAttachToPanelEvent(AttachToPanelEvent evt)
    {
        this.Q<VisualElement>("Options").RegisterCallback<MouseDownEvent>(OnMouseDown);
    }

    private void OnDetachFromPanelEvent(DetachFromPanelEvent evt)
    {
        this.Q<VisualElement>("Options").UnregisterCallback<MouseDownEvent>(OnMouseDown);
    }

    public void BindProperty(SerializedProperty property, Type fieldType)
    {
        if(property == null || fieldType == null)        
            return;

        this.fieldType = fieldType;
        this.property = property;        

        this.Q<PropertyField>().BindProperty(property);

        if(property.objectReferenceValue == null)
            return;

        this.serializedObject = new SerializedObject(property.objectReferenceValue);

        BindSerializedObject(this.serializedObject);
    }

    void CreateScriptableObject()
    {               
        var scriptableObject = ScriptableObject.CreateInstance(this.fieldType);

        if(this.serializedObject == null)
            throw new ArgumentException("There is no serialized object refrenced.");

        if(scriptableObject == null)
            throw new ArgumentException("Failed to create Scriptable Object, check usage of this script.");

        this.property.objectReferenceValue = scriptableObject;
        this.property.serializedObject.ApplyModifiedProperties();

        this.serializedObject = new SerializedObject(this.property.objectReferenceValue);

        BindSerializedObject(this.serializedObject);
    }

    void BindSerializedObject(SerializedObject serializedObject)
    {
        /// let any value through to let connected binds handle any state changes
        this.bind?.BindSerializedObject(serializedObject);
        this.additionalBindingAction?.Invoke(this.serializedObject);

        this.iJSON = serializedObject?.targetObject as IJSON;
    }

    void OnMouseDown(MouseDownEvent evt)
    {
        GenericMenu menu = new GenericMenu();
       
        if(this.iJSON != null)
        {
            menu.AddDisabledItem(new GUIContent("-- JSON --"));

            if (HasJSONTable())
            {
                menu.AddItem(new GUIContent("Read From JSON"), false, () =>
                {
                    FromJSON();
                });

                menu.AddItem(new GUIContent("Write To JSON"), false, () =>
                {
                    ToJSON();
                });

                menu.AddSeparator("");

                menu.AddItem(new GUIContent("Open JSON File"), false, () =>
                {
                    OpenJSONFile();
                });
            }   
        
            menu.AddItem(new GUIContent("Create JSON File"), false, () =>
            {
                CreateJSONTable();
            });        

            menu.AddItem(new GUIContent("Reference JSON File"), false, () =>
            {
                ReferenceJSONTableReference();
            });

            menu.AddItem(new GUIContent("Ping JSON File"), false, () =>
            {
                PingJSONFile();
            });

            menu.AddSeparator("");
        }

        menu.AddDisabledItem(new GUIContent("-- Scriptable Object --"));

        menu.AddItem(new GUIContent("Create Scriptable Object"), false, () =>
        {
            CreateScriptableObject();
        });

        menu.AddItem(new GUIContent("Save Scriptable Object As Asset"), false, () =>
        {
            SaveScriptableObjectAsset();
        });

        menu.ShowAsContext();
    }

    private void PingJSONFile()
    {
        if (this.iJSON == null)
            return;

        EditorGUIUtility.PingObject(this.iJSON.TextAsset);
    }

    void SaveScriptableObjectAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Scriptable Object", "New Scriptable Object", "asset", "");

        if (string.IsNullOrEmpty(path))
            return;

        ScriptableObject newScriptableObject = ScriptableObject.Instantiate(this.serializedObject.targetObject) as ScriptableObject;

        AssetDatabase.CreateAsset(newScriptableObject, path);
        AssetDatabase.SaveAssets();

        EditorGUIUtility.PingObject(newScriptableObject);

        this.property.objectReferenceValue = newScriptableObject;
        this.property.serializedObject.ApplyModifiedProperties();

        BindSerializedObject(new SerializedObject(newScriptableObject));
    }

    void CreateJSONTable()
    {
        string path = EditorUtility.SaveFilePanelInProject("", "", "json", "");

        if (string.IsNullOrEmpty(path))
            return;

        path.Replace(Application.dataPath, "Assets");

        TextAsset textAsset = new TextAsset();        


        File.CreateText(path).Close();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(textAsset);

        this.iJSON.TextAsset = AssetDatabase.LoadAssetAtPath<Object>(path) as TextAsset;    
        this.serializedObject.ApplyModifiedProperties();

        ToJSON();
    }

    void ReferenceJSONTableReference()
    {
        string path = EditorUtility.OpenFilePanel("", "", "json");

        if (string.IsNullOrEmpty(path))
            return;

        path = path.Replace(Application.dataPath, "Assets");

        Object textAsset = AssetDatabase.LoadAssetAtPath<Object>(path);

        if (textAsset != null)        
            this.iJSON.TextAsset = textAsset as TextAsset;       
    }

    void ToJSON()
    {
        if (!HasJSONTable())
            return;

        File.WriteAllText(AssetDatabase.GetAssetPath(this.iJSON.TextAsset), JsonUtility.ToJson(this.serializedObject.targetObject, true));
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    void FromJSON()
    {
        if (!HasJSONTable())
            return;

        JsonUtility.FromJsonOverwrite(this.iJSON.TextAsset.text, this.serializedObject.targetObject);
        this.serializedObject.Update();
        this.serializedObject.ApplyModifiedProperties();

        BindSerializedObject(this.serializedObject);
    }

    bool HasJSONTable()
    {
        if (this.iJSON.TextAsset == null)
            return false;

        string path = AssetDatabase.GetAssetPath(this.iJSON.TextAsset);

        if (!path.Contains(".json"))
            return false;


        return true;
    }

    void OpenJSONFile()
    {
        AssetDatabase.OpenAsset(this.iJSON.TextAsset);
    }
}