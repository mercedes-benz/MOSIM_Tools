using MMICSharp.Access;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MMIUnity;

using Newtonsoft.Json;

public class AutoLoad : UnityAvatarBase
{
    // Start is called before the first frame update
    override
    protected void Start()
    {
        base.Start();

        string id = "asdf";

        string s = System.IO.File.ReadAllText("test_skeleton_config.retargeting");
        MAvatarPosture p = JsonConvert.DeserializeObject<MAvatarPosture>(s);
        p.AvatarID = id;

        this.SetupRetargeting(id, p);
        this.AssignPostureValues(retargetingService.RetargetToIntermediate(p));

        MAvatarPosture z = this.GetSkeletonAccess().GetAvatarDescription(id).ZeroPosture;
        MAvatarPosture z2 = this.GetZeroPosture();
    }
}
