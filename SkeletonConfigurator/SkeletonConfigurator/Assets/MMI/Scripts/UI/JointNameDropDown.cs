using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MMIStandard;

public class JointNameDropDown : Dropdown
{
    public KeyCode enableSelection = KeyCode.S;
    public MJointType jointType = MJointType.Undefined;
    public Camera mainCamera;
    public bool isSelectable {get; private set;} = false;
    override public void OnPointerClick(PointerEventData eventData)
    {
        Show();
        isSelectable = true;
    }

    void Update()
    {
        if(!Input.GetKey(enableSelection))
            isSelectable = false;
        else if(Input.GetMouseButtonDown(0) && isSelectable)
        {
            RaycastHit hitInfo; 
			if(Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                Transform target = hitInfo.transform.parent.parent;
                if(target.GetComponent<JointInfo>())
                {
                    int idx = options.FindIndex(x => x.text == target.name);
                    if(idx != -1)
                        value = idx;
                }
            }
        }
    }

}
