// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// [System.Serializable]
// public class Tenant : IAttributeModif
// {
//     public string name;
//     public string color;
//     public string mainContact;
//     public string mainPhone;
//     public string mainEmail;
//     public string id;

//     public Tenant(string _name, string _color)
//     {
//         name = _name;
//         color = _color;
//     }

//     ///<summary>
//     /// Check for a _param attribute and assign _value to it.
//     ///</summary>
//     ///<param name="_param">The attribute to modify</param>
//     ///<param name="_value">The value to assign</param>
//     public void SetAttribute(string _param, string _value)
//     {
//         switch (_param)
//         {
//             case "mainContact":
//                 mainContact = _value;
//                 break;
//             case "mainPhone":
//                 mainPhone = _value;
//                 break;
//             case "mainEmail":
//                 mainEmail = _value;
//                 break;
//             default:
//                 GameManager.gm.AppendLogLine($"[Tenant] {name}: unknowed attribute to update.", "yellow");
//                 break;
//         }

//         Customer cu = GameObject.Find(name)?.GetComponent<Customer>();
//         if (cu)
//             cu.PutData();

//     }

//     ///<summary>
//     /// Update Id of corresponding Customer if existing
//     ///</summary>
//     ///<param name="_id">The id to set</param>
//     public void UpdateId(string _id)
//     {
//         id = _id;
//         Customer cu = GameObject.Find(name)?.GetComponent<Customer>();
//         if (cu)
//             cu.GetComponent<AServerItem>().id = id;
//     }

// }
