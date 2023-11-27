rule ADOKit_Signatures
{
    meta:
        description = "Static signatures for the ADOKit tool."
        md5 = "9b4b2a06aa840afcbbfe2d412f99b4a9"
        rev = 1
        author = "Brett Hawkins"
    strings:
        $typelibguid = "60BC266D-1ED5-4AB5-B0DD-E1001C3B1498" ascii nocase wide

    condition:
        uint16(0) == 0x5A4D and $typelibguid
}
