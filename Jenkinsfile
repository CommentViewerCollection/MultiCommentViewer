node('JenkinsSlave') {
	stage('cleanup') {
		deleteDir()
	}
	stage('build') {
		bat 'git clone --recursive git@github.com:CommentViewerCollection/MultiCommentViewer.git'
		dir('./MultiCommentViewer') {
			bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community\\Common7\\Tools\\VsDevCmd.bat"'
			bat 'nuget restore MultiCommentViewer.sln'
			bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community\\MSBuild\\15.0\\Bin\\MSBuild.exe" MultiCommentViewer.sln'
		}
	}

	stage('test') {
		dir('./MultiCommentViewer') {
			bat '"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community\\Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\vstest.console.exe" MultiCommentViewerTests\\bin\\Debug\\MultiCommentViewerTests.dll'
		}
	}
}
