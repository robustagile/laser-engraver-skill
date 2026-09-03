# ---------------------------------------------------------------------------
# Deploys the laser-engraver plugin into a .claude directory. The Windows
# counterpart of install.sh, and it must stay in step with it.
#
# It copies files and does nothing else. That is deliberate and it is the
# reason this is a script rather than a .NET program: the .NET SDK is exactly
# the thing that may not be installed yet, so the installer cannot be written
# in it. The SDK check belongs to the skill, on first use. (R-N8)
#
# Two rules it must never break:
#
#   1. Everything under skills\ and commands\ is disposable and is replaced
#      wholesale. Everything under laser-engraver\ belongs to the user and is
#      never read, written or looked into by this script. A recipe base is
#      weeks of test burns; an installer that reached into it would destroy
#      the one thing the plugin exists to build. (R-N7)
#
#   2. Re-running it is safe, and it says what it changed. (R-N7)
#
# Usage: .\install.ps1 <target> [-Link]
# ---------------------------------------------------------------------------
[CmdletBinding()]
param(
    [Parameter(Position = 0)][string] $Target,
    [switch] $Link,
    [switch] $Help
)

$ErrorActionPreference = 'Stop'

# Our own failures are messages, not PowerShell error records: Write-Error
# under $ErrorActionPreference = 'Stop' is itself terminating, which buries a
# plain instruction under a stack trace and never reaches the exit code.
function Fail {
    param([string] $Message, [int] $Code = 1)
    [Console]::Error.WriteLine($Message)
    exit $Code
}

$source   = $PSScriptRoot
$payload  = Join-Path $source 'plugin'
$skills   = @('laser-machines', 'laser-lightburn')
$commands = @('laser-machine', 'laser-recipes', 'laser-job', 'laser-fix')
$dataDir  = 'laser-engraver'

function Show-Usage {
    Write-Host @'
Usage: .\install.ps1 <target> [-Link]

  <target>   the .claude directory to install into:
               $HOME\.claude        available in every directory
               <project>\.claude    confined to one working folder

  -Link      developer mode: symlink the skills to this clone instead of
             copying them, so an edit here takes effect with no re-install.
             Windows needs developer mode or an elevated prompt to create a
             symlink. Not for normal use - `git pull` then changes the
             installed plugin underneath you, which is the point of it.

Your machine records, recipes and burn results live in <target>\laser-engraver
and are never touched. Re-run this to update.
'@
}

if ($Help) { Show-Usage; exit 0 }

if ([string]::IsNullOrWhiteSpace($Target)) {
    [Console]::Error.WriteLine('install.ps1: no target given.')
    Show-Usage
    exit 2
}

# --- sanity: are we in a clone, and is the target plausible? ---------------

foreach ($skill in $skills) {
    $marker = Join-Path (Join-Path (Join-Path $payload 'skills') $skill) 'SKILL.md'
    if (-not (Test-Path -LiteralPath $marker -PathType Leaf)) {
        Fail "install.ps1: $marker is missing. Run this script from the repository it came with."
    }
}

New-Item -ItemType Directory -Force -Path $Target | Out-Null
$Target = (Resolve-Path -LiteralPath $Target).ProviderPath.TrimEnd('\')

if ((Split-Path -Leaf $Target) -ne '.claude') {
    Write-Host "Note: $Target is not named .claude. Installing there anyway - but if"
    Write-Host "      that was a slip, nothing you meant to keep has been touched yet."
    Write-Host ''
}

# --- what we are installing, so the report can name it --------------------

# git is not a prerequisite of the installer (R-N8), and outside a repository
# it writes to stderr and exits non-zero. Under $ErrorActionPreference = 'Stop'
# either of those aborts the script, so every call goes through here: stderr
# discarded, the exit code returned as data, a missing git treated as an
# absent answer rather than an error.
function Invoke-Git {
    param([string] $Directory, [string[]] $Arguments, [switch] $CodeOnly)
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $output = & git -C $Directory @Arguments 2>$null
        $code = $LASTEXITCODE
    } catch {
        return $null            # git is not installed
    } finally {
        $ErrorActionPreference = $previous
    }
    if ($CodeOnly) { return $code }
    if ($code -ne 0) { return $null }
    return $output
}

function Invoke-GitInSource {
    param([string[]] $Arguments)
    return Invoke-Git -Directory $source -Arguments $Arguments
}

$newCommit = Invoke-GitInSource @('rev-parse', '--short', 'HEAD')
if ($null -eq $newCommit) {
    $newCommit = 'unknown'
    $newState  = 'unknown'
} else {
    $newCommit = ($newCommit | Select-Object -First 1).Trim()
    $dirty     = Invoke-GitInSource @('status', '--porcelain', '--', 'plugin')
    if ([string]::IsNullOrWhiteSpace(($dirty -join ''))) { $newState = 'clean' } else { $newState = 'modified' }
}

function Get-InstalledCommit {
    param([string] $SkillPath)
    $file = Join-Path $SkillPath 'VERSION'
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { return $null }
    foreach ($line in Get-Content -LiteralPath $file) {
        if ($line -match '^commit:\s*(\S+)') { return $Matches[1] }
    }
    return $null
}

$described = $newCommit
if ($newState -eq 'modified') { $described = "$newCommit, working tree modified" }

$userLevel = ($Target -eq (Join-Path $HOME '.claude'))

Write-Host 'Installing the laser-engraver plugin'
Write-Host ''
Write-Host "  source   $source  ($described)"
if ($userLevel) {
    Write-Host "  target   $Target  (user level)"
} else {
    Write-Host "  target   $Target  (project level)"
}
Write-Host ''

# --- the skills: replaced wholesale ---------------------------------------

$skillsRoot   = Join-Path $Target 'skills'
$commandsRoot = Join-Path $Target 'commands'
New-Item -ItemType Directory -Force -Path $skillsRoot, $commandsRoot | Out-Null

foreach ($skill in $skills) {
    $dest = Join-Path $skillsRoot $skill
    $was  = Get-InstalledCommit $dest
    $src  = Join-Path (Join-Path $payload 'skills') $skill

    if (Test-Path -LiteralPath $dest) {
        # Guard the removal on the name we just built, never on a variable
        # that could have come out empty.
        if ((Split-Path -Leaf $dest) -ne $skill) {
            Fail "install.ps1: refusing to remove $dest"
        }
        # A symlinked skill from a -Link install: remove the link, never what
        # it points at.
        $item = Get-Item -LiteralPath $dest -Force
        if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            $item.Delete()
        } else {
            Remove-Item -LiteralPath $dest -Recurse -Force
        }
    }

    if ($Link) {
        try {
            New-Item -ItemType SymbolicLink -Path $dest -Target $src -ErrorAction Stop | Out-Null
        } catch {
            Fail @"
install.ps1: could not create a symlink at $dest.
             Windows allows this only with developer mode enabled or from an
             elevated prompt. Run without -Link to copy the files instead.
"@
        }
        Write-Host ("  {0,-26} {1}" -f "skills\$skill", 'linked to the clone')
        continue
    }

    Copy-Item -LiteralPath $src -Destination $dest -Recurse

    # Build output is not payload: the generator is built from source on the
    # user's machine, and a stale bin\ carried over from here would be the one
    # copy nobody thought to look at.
    Get-ChildItem -LiteralPath $dest -Recurse -Directory -Force |
        Where-Object { $_.Name -in 'bin', 'obj' } |
        ForEach-Object { if (Test-Path -LiteralPath $_.FullName) { Remove-Item -LiteralPath $_.FullName -Recurse -Force } }

    $stamp = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
    @(
        "commit:    $newCommit"
        "state:     $newState"
        "installed: $stamp"
        "source:    $source"
    ) | Set-Content -LiteralPath (Join-Path $dest 'VERSION') -Encoding UTF8

    if ($null -eq $was)                 { $note = 'installed' }
    elseif ($was -ne $newCommit)        { $note = "updated from $was" }
    elseif ($newState -eq 'modified')   { $note = 're-copied (same commit, working tree modified)' }
    else                                { $note = "re-copied (already $newCommit)" }

    Write-Host ("  {0,-26} {1}" -f "skills\$skill", $note)
}

# --- the commands ---------------------------------------------------------

foreach ($cmd in $commands) {
    $src  = Join-Path (Join-Path $payload 'commands') "$cmd.md"
    $dest = Join-Path $commandsRoot "$cmd.md"

    if (-not (Test-Path -LiteralPath $dest -PathType Leaf)) {
        $note = 'installed'
    } elseif ((Get-FileHash -LiteralPath $src).Hash -eq (Get-FileHash -LiteralPath $dest).Hash) {
        $note = 'unchanged'
    } else {
        $note = 'updated'
    }
    if ($note -ne 'unchanged') { Copy-Item -LiteralPath $src -Destination $dest -Force }
    Write-Host ("  {0,-26} {1}" -f "commands\$cmd.md", $note)
}

# A laser-*.md we did not just write is either from a version that had a
# command this one does not, or the user's own. Both are their call, so say so
# rather than deleting someone's file.
$known = $commands | ForEach-Object { "$_.md" }
Get-ChildItem -LiteralPath $commandsRoot -Filter 'laser-*.md' -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -notin $known } |
    ForEach-Object {
        Write-Host ("  {0,-26} {1}" -f "commands\$($_.Name)", 'left in place - not part of this version. Delete it if it')
        Write-Host ("  {0,-26} {1}" -f '', 'came from an older install.')
    }

# --- the user's data directory: named, never touched ----------------------

$dataPath = Join-Path $Target $dataDir
Write-Host ''
if (Test-Path -LiteralPath $dataPath -PathType Container) {
    Write-Host 'Your data directory is where it was, untouched:'
    Write-Host "  $dataPath"
} else {
    Write-Host "Your data will live in $dataPath"
    Write-Host '  - it does not exist yet; the skill creates it the first time it has'
    Write-Host '    something of yours to write.'
}

# --- project level: keep records out of the project's history -------------

if (-not $userLevel) {
    $repo = Invoke-Git -Directory $Target -Arguments @('rev-parse', '--show-toplevel')
    if (-not [string]::IsNullOrWhiteSpace(($repo -join ''))) {
        $repo = ($repo | Select-Object -First 1).Trim()
        # The trailing slash matters: a directory-only pattern is not matched
        # by check-ignore unless the queried path carries one, and this
        # directory usually does not exist yet at install time.
        $ignored = Invoke-Git -Directory $repo -CodeOnly `
                              -Arguments @('check-ignore', '-q', "$dataPath/")
        if ($ignored -ne 0) {
            $repoNative = $repo.Replace('/', [IO.Path]::DirectorySeparatorChar)
            $relative   = $dataPath.Substring($repoNative.Length).TrimStart('\', '/').Replace('\', '/')
            Write-Host ''
            Write-Host 'This .claude is inside a git repository, and a project''s .claude is'
            Write-Host 'commonly committed. Your machine records and burn results should not'
            Write-Host "be. Add this to $repoNative\.gitignore:"
            Write-Host ''
            Write-Host "  $relative/"
        }
    }
}

Write-Host ''
if ($Link) {
    Write-Host 'Done - developer mode. The skills point at this clone, so edits here are'
    Write-Host 'live and a git pull changes the installed plugin.'
} else {
    Write-Host 'Done. To update: git pull here, then run this again with the same target.'
}
