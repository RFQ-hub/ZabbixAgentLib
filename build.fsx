#I "packages/build/FAKE/tools"
#r "FakeLib.dll"

open Fake

let configuration = environVarOrDefault "Configuration" (if buildServer = LocalBuild then "Debug" else "Release")
let solution = "ZabbixAgent.sln"

let buildNumber =
    [
        TeamCityHelper.TeamCityBuildParameters.tryGet "build.counter"
        environVarOrNone "APPVEYOR_BUILD_NUMBER"
        Some "0"
    ]
    |> List.choose id |> List.tryHead

let dotNetAdditionalArgs =
    match buildNumber with
    | Some buildNumber -> [ sprintf "/p:BuildNumber=%s" buildNumber ]
    | _ -> []

Target "build" <| fun _ ->
    DotNetCli.Build (fun p ->
        { p with
            WorkingDir = "src"
            Configuration = configuration
            Project = solution
            AdditionalArgs = dotNetAdditionalArgs
        })

Target "pack" <| fun _ ->
    DotNetCli.Pack (fun p ->
        { p with
            WorkingDir = "src"
            Configuration = configuration
            Project = solution
            AdditionalArgs = dotNetAdditionalArgs
        })

Target "ci" <| fun _ ->
    !! ("src\\**\\*.nupkg") |> Seq.iter TeamCityHelper.PublishArtifact
    !! ("src\\**\\*.nupkg") |> AppVeyor.PushArtifacts

"build" ==> "pack"
"pack" ==> "ci"

RunTargetOrDefault "build"
