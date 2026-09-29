#Requires -Version 7.0
param([string]$BaseUrl = 'http://localhost:5000')

$ErrorActionPreference = 'Stop'
$BaseUrl = $BaseUrl.TrimEnd('/')

function Assert-Condition([bool]$Condition, [string]$Message) {
    if (!$Condition) {
        throw $Message
    }
}

function Read-JsonResponse($Response) {
    $content = $Response.Content
    if ($content -is [byte[]]) {
        $content = [Text.Encoding]::UTF8.GetString($content)
    }
    return $content | ConvertFrom-Json
}

$invalidCases = @(
    @{ Name = 'hole crossing shell'; Geometry = '{"type":"Polygon","coordinates":[[[0,0],[20,0],[20,20],[0,20],[0,0]],[[10,10],[30,10],[30,30],[10,30],[10,10]]]}' },
    @{ Name = 'hole outside shell'; Geometry = '{"type":"Polygon","coordinates":[[[0,0],[20,0],[20,20],[0,20],[0,0]],[[30,30],[40,30],[40,40],[30,40],[30,30]]]}' },
    @{ Name = 'self-intersecting polygon'; Geometry = '{"type":"Polygon","coordinates":[[[0,0],[20,20],[0,20],[20,0],[0,0]]]}' },
    @{ Name = 'overlapping multipolygon'; Geometry = '{"type":"MultiPolygon","coordinates":[[[[0,0],[20,0],[20,20],[0,20],[0,0]]],[[[10,10],[30,10],[30,30],[10,30],[10,10]]]]}' },
    @{ Name = 'invalid nested component'; Geometry = '{"type":"GeometryCollection","geometries":[{"type":"Point","coordinates":[10,10]},{"type":"GeometryCollection","geometries":[{"type":"Polygon","coordinates":[[[0,0],[20,20],[0,20],[20,0],[0,0]]]}]}]}' }
)

foreach ($case in $invalidCases) {
    $body = '{"geometry":' + $case.Geometry + ',"zoom":3}'
    $response = Invoke-WebRequest "$BaseUrl/covers" -Method Post -ContentType 'application/json' -Body $body -SkipHttpErrorCheck
    Assert-Condition ($response.StatusCode -eq 400) "$($case.Name): expected 400, got $($response.StatusCode)"
    $problem = Read-JsonResponse $response
    Assert-Condition ($problem.status -eq 400) "$($case.Name): missing problem status"
    Assert-Condition ($problem.errors.Geometry[0] -like 'Invalid geometry: *') "$($case.Name): missing topology reason"
    Assert-Condition (!($problem.PSObject.Properties.Name -contains 'coverId')) "$($case.Name): invalid geometry created a cover"
    Write-Output "PASS $($case.Name)"
}

$validCases = @(
    @{ Name = 'point'; Geometry = '{"type":"Point","coordinates":[10,10]}' },
    @{ Name = 'self-crossing line'; Geometry = '{"type":"LineString","coordinates":[[0,0],[20,20],[0,20],[20,0]]}' },
    @{ Name = 'polygon with hole'; Geometry = '{"type":"Polygon","coordinates":[[[0,0],[20,0],[20,20],[0,20],[0,0]],[[5,5],[5,15],[15,15],[15,5],[5,5]]]}' },
    @{ Name = 'disjoint multipolygon'; Geometry = '{"type":"MultiPolygon","coordinates":[[[[0,0],[10,0],[10,10],[0,10],[0,0]]],[[[20,20],[30,20],[30,30],[20,30],[20,20]]]]}' },
    @{ Name = 'overlapping collection components'; Geometry = '{"type":"GeometryCollection","geometries":[{"type":"Polygon","coordinates":[[[0,0],[20,0],[20,20],[0,20],[0,0]]]},{"type":"Polygon","coordinates":[[[10,10],[30,10],[30,30],[10,30],[10,10]]]}]}' },
    @{ Name = 'empty polygon'; Geometry = '{"type":"Polygon","coordinates":[]}' ; Empty = $true },
    @{ Name = 'empty collection'; Geometry = '{"type":"GeometryCollection","geometries":[]}' ; Empty = $true }
)

foreach ($case in $validCases) {
    foreach ($includeBounds in @('false', 'true')) {
        $body = '{"geometry":' + $case.Geometry + ',"zoom":3}'
        $response = Invoke-WebRequest "$BaseUrl/covers?includeBounds=$includeBounds" -Method Post -ContentType 'application/json' -Body $body -SkipHttpErrorCheck
        Assert-Condition ($response.StatusCode -eq 200) "$($case.Name): expected 200, got $($response.StatusCode)"
        $cover = Read-JsonResponse $response
        Assert-Condition (![string]::IsNullOrEmpty($cover.coverId)) "$($case.Name): missing cover ID"
        Assert-Condition (($cover.tiles.Count -eq 0) -eq ($case.Empty -eq $true)) "$($case.Name): unexpected empty result"
        $cached = Invoke-RestMethod "$BaseUrl/covers/$($cover.coverId)/tiles?zoom=3&includeBounds=$includeBounds"
        Assert-Condition ($cached.coverId -eq $cover.coverId) "$($case.Name): cover ID changed"
        Assert-Condition (($cover.tiles | ConvertTo-Json -Depth 10 -Compress) -eq ($cached.tiles | ConvertTo-Json -Depth 10 -Compress)) "$($case.Name): cached tiles differ"
        foreach ($tile in $cover.tiles) {
            Assert-Condition (!($tile.PSObject.Properties.Name -contains 'id')) "$($case.Name): packed ID is still serialized"
            Assert-Condition ($tile.z -eq 3 -and $tile.x -ge 0 -and $tile.x -lt 8 -and $tile.y -ge 0 -and $tile.y -lt 8) "$($case.Name): invalid XYZ tile"
            Assert-Condition (($tile.PSObject.Properties.Name -contains 'bounds') -eq ($includeBounds -eq 'true')) "$($case.Name): unexpected bounds field"
        }
        Write-Output "PASS $($case.Name), includeBounds=$includeBounds, cached GET"
    }
}

Write-Output 'All 19 HTTP regression cases passed.'
