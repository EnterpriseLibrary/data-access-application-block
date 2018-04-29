function Add-ToolFolder([System.String] $folder)
{
    $flattenedValue = Get-ExtenderPropertyValue
    
    $allFolders = New-Object "System.Collections.Generic.List``1[System.String]"
    
    if($flattenedValue.Length -gt 0)
    {
        $allFolders.AddRange($flattenedValue.Split(';'))
    }
    
    if( -not $allFolders.Contains($folder) )
    {
        $allFolders.Add($folder)
        
        $flattenedValue = [System.String]::Join(';', $allFolders.ToArray())
        Set-ExtenderPropertyValue($flattenedValue)
    }
}

function Remove-ToolFolder([System.String] $folder)
{
    $flattenedValue = Get-ExtenderPropertyValue
    
    $allFolders = New-Object "System.Collections.Generic.List``1[System.String]"
    
    if($flattenedValue.Length -gt 0)
    {
        $allFolders.AddRange($flattenedValue.Split(';'))
    }
    
    if( $allFolders.Remove($folder) )
    {
        $flattenedValue = [System.String]::Join(';', $allFolders.ToArray())
        Set-ExtenderPropertyValue($flattenedValue)
    }
}


function Get-ExtenderPropertyValue
{
    if( $dte.Solution.Globals.VariableExists("EnterpriseLibraryConfigurationToolBinariesPathV6") -and $dte.Solution.Globals.VariablePersists("EnterpriseLibraryConfigurationToolBinariesPathV6") )
    {
        return $dte.Solution.Globals.VariableValue("EnterpriseLibraryConfigurationToolBinariesPathV6")
    }

    
    return ""
}

function Set-ExtenderPropertyValue([System.String] $value)
{
    if( [System.String]::IsNullOrWhiteSpace($value) )
    {
        if( $dte.Solution.Globals.VariableExists("EnterpriseLibraryConfigurationToolBinariesPathV6") )
        {
            $dte.Solution.Globals.VariablePersists("EnterpriseLibraryConfigurationToolBinariesPathV6") = $False
        }
    }
    else
    {
        $dte.Solution.Globals.VariableValue("EnterpriseLibraryConfigurationToolBinariesPathV6") = $value
        $dte.Solution.Globals.VariablePersists("EnterpriseLibraryConfigurationToolBinariesPathV6") = $True
    }
}

function Get-RelativePath([System.String] $basePath, [System.String] $targetPath)
{
    # not a general purpose relative path calculation algorithm
    
    return ($targetPath.Substring($basePath.Length)).TrimStart([System.Io.Path]::DirectorySeparatorChar)
}

function Cleanup
{
}
