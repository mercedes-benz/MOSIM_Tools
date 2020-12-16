// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using MMIStandard;
using MMICSharp.Common.Communication;
using MMIUnity;


/// <summary>
/// Window for initial generation of the MMU structure and description.
/// </summary>
public class SetupMMUWindow : EditorWindow
{
    private enum State
    {
        CreateStructure,
        SetupComponents
    }

    private State state = State.CreateStructure;


    private bool useAnimatorTemplate = false;

    private bool createPrefab = true;


    /// <summary>
    /// The assigned description file
    /// </summary>
    private readonly MMUDescription description = new MMUDescription();



    void OnGUI()
    {
        if (state == State.CreateStructure)
        {
            EditorGUILayout.LabelField("Please enter the relevant description of the desired MMU:");
            description.Name = EditorGUILayout.TextField("Name", description.Name);
            description.MotionType = EditorGUILayout.TextField("MotionType", description.MotionType);
            description.Author = EditorGUILayout.TextField("Author", description.Author);
            description.ShortDescription = EditorGUILayout.TextField("ShortDescription", description.ShortDescription);
            description.LongDescription = EditorGUILayout.TextField("LongDescription", description.LongDescription);
            if(description.Version == null)
                description.Version = "1.0";
            
            this.useAnimatorTemplate = EditorGUILayout.Toggle("Use Animator Template", this.useAnimatorTemplate);
        }

        if (state == State.SetupComponents)
        {
            this.createPrefab = EditorGUILayout.Toggle("Create prefab", this.createPrefab);
        }

        //To do automatically detect the gameobject if one is selected
        GameObject active = Selection.activeGameObject;

        //Use the name of the gameobject
        if (active != null)
        {
            description.Name = active.name;
        }

        switch (state)
        {
            case State.CreateStructure:
                if (GUILayout.Button("Create Structure"))
                {
                    Directory.CreateDirectory("Assets//" + description.Name);
                    Directory.CreateDirectory("Assets//" + description.Name + "//Dependencies");
                    Directory.CreateDirectory("Assets//" + description.Name + "//Scripts");


                    //Create a unique id
                    description.ID = System.Guid.NewGuid().ToString();
                    description.Language = "UnityC#";
                    description.Dependencies = new List<MDependency>();
                    description.AssemblyName = description.Name + ".dll";

                    //Generate the .cs file
                    GenerateTemplateClass(description, true, this.useAnimatorTemplate);

                    //Store the description
                    System.IO.File.WriteAllText("Assets//" + description.Name + "//description.json", Serialization.ToJsonString(description));

                    //Refresh the asset database to show the new filestructure
                    AssetDatabase.Refresh();

                    Debug.Log("File structure for MMU " + description.Name + " successfully created!");

                    this.state = State.SetupComponents;

                    EditorUtility.DisplayDialog("MMU structure successfully created.", "The MMU structure has been successfully created.", "Continue");

                }
                break;


            case State.SetupComponents:
                if (GUILayout.Button("Setup Components"))
                {
                    if (active != null)
                    {
                        //Add the script directly to the object
                        Component component = active.AddComponent(System.Type.GetType(description.Name));

                        //Do the initialization
                        AutoCodeGenerator.SetupBoneMapping();
                        AutoCodeGenerator.AutoGenerateScriptInitialization();

                        //Assign the game joint prefab
                        active.GetComponent<UnityAvatarBase>().gameJointPrefab = Resources.Load("singleBone") as GameObject;


                        //Create prefab if desired
                        if (createPrefab)
                        {
                            bool success = false;

                            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(active, "Assets//" + description.Name + "//" + description.Name + ".prefab", out success);

                            Debug.Log("Creating prefab: " + success);
                        }

                        AssetDatabase.Refresh();

                        Debug.Log("Components for MMU " + description.Name + " successfully set up!");

                        //Close the window
                        Close();

                        EditorUtility.DisplayDialog("MMU components successfully added.", "The MMU components have been successfully added.", "Continue");

                    }

                    else
                    {
                        EditorUtility.DisplayDialog("No object selected.", "Please select the desired MMU object in order to setup a MMU.", "Continue");
                    }
                }

                break;
        }


        if (GUILayout.Button("Close"))
            Close();
    }


    [MenuItem("MMI/Setup MMU", false, 0)]
    static void SetupMMUAssets()
    {
        SetupMMUWindow window = new SetupMMUWindow();
        window.ShowUtility();
    }

    [MenuItem("GameObject/Setup MMU", false,0)]
    static void SetupMMU()
    {
        SetupMMUWindow window = new SetupMMUWindow();
        window.ShowUtility();
    }


    static void GenerateTemplateClass(MMUDescription description, bool useBaseClass, bool useAnimatorTemplate)
    {
        //Get the template for the auto-generated MMU class
        string mmuTemplate;


        if (useAnimatorTemplate)
        {
            mmuTemplate = System.IO.File.ReadAllText(@"Assets\MMUGenerator\Data\MMUTemplateBaseClassAnimator.template");
        }
        else
        {
            mmuTemplate = System.IO.File.ReadAllText(@"Assets\MMUGenerator\Data\MMUTemplateBaseClass.template");
        }

        


        //Replace the placeholders
        mmuTemplate = mmuTemplate.Replace("CLASS_NAME", description.Name);
        mmuTemplate = mmuTemplate.Replace("MOTION_TYPE", description.MotionType);


        //Write the class to the location
        System.IO.File.WriteAllText("Assets//" + description.Name + "//Scripts//" + description.Name + ".cs", mmuTemplate);
    }

}

#endif

