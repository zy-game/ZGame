using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestMovePath : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //当按下鼠标左键发射射线，检测是否击中background层级的物体，如果是，或者玩家身上的导航组件，并设置导航目标位置
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 1000, 1 << LayerMask.NameToLayer("background")))
            {
                transform.GetComponent<NavMeshAgent>().SetDestination(hitInfo.point);
            }
        }
    }
}