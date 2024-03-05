// ********************************************************************
// CONFIDENTIAL - DO NOT DISTRIBUTE
// COPYRIGHT 2019-2024 Wacky Potato Games, LLC. All Rights Reserved.
// 
// If you send, receive, or use this file for any purpose other than
// internal use by Wacky Potato Games, it is without permission and an act of theft.
// Report any misuse of this file immediately to contact@wackypotato.com
// Misuse or failure to report misuse will subject you to legal action.
// 
// The intellectual and technical concepts contained herein are
// proprietary and are protected by trade secret and/or copyright law.
// Dissemination or reproduction of this material is forbidden.
// ********************************************************************

public class Minigun : FiringWeapon
{
    public override WeaponType WeaponType => WeaponType.Minigun;

    public override AmmoType AmmoType => AmmoType.Bullet;

    //Notes for me. I pulled out the loop sound in exchange for a 
    //short easy sfx on burst

    private bool _spinning;   
    
    protected override void Update()
    {
        base.Update();
        if(_spinning && !IsFiring)
        {
            _spinning = false;
            StopBarrel();            
        }
    }

    public override void PrimaryAttack()
    {
        base.PrimaryAttack();
        if (IsFiring && !_spinning)
        {
            _spinning = true;
            animator.Play("SpinBarrel");
        }
    }    

    public void StopBarrel()
    {
        _spinning = false;        
        animator.Play("SpinDown");
    }        

}
