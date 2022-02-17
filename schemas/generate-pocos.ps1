Write-Host '------------------------------------'
Write-Host ' Generating POCOs from Avro schemas'
Write-Host '------------------------------------'

Write-Host 'Checking dependencies...'
$avrogenExists = dotnet tool list --global | Out-String -Stream | Select-String 'avrogen' -SimpleMatch -Quiet
$dotnet2Exists = dotnet --list-runtimes | Out-String -Stream | Select-String 'Microsoft.NETCore.App 2.1.' -SimpleMatch -Quiet

if ($avrogenExists -and $dotnet2Exists) {
	Write-Host '  All required dependencies available.'
} 
else {
	Write-Host '  Required dependencies not installed.'
    Write-Host '  Press any key to continue and install, or CTRL+C to quit:' -ForegroundColor yellow -NoNewLine; Read-Host

	Write-Host 'Installing dependencies...'
	# Install .NET first if it doesn't exist (else AvroGen install will also fail)
	if (-not $dotnet2Exists) {
		&powershell -NoProfile -ExecutionPolicy unrestricted -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; &([scriptblock]::Create((Invoke-WebRequest -UseBasicParsing 'https://dot.net/v1/dotnet-install.ps1'))) -Runtime dotnet -Version '2.1.30'"
	}
	if (-not $avrogenExists) {
		&dotnet tool install --global Confluent.Apache.Avro.AvroGen 
	}
}

Write-Host ''
Write-Host 'Generating POCOs...'
avrogen -s CustomerLoyalty.avsc ..\samples\GBB.SchemaRegistry.Consumer\
avrogen -s CustomerLoyalty.avsc ..\samples\GBB.SchemaRegistry.Functions\
avrogen -s CustomerLoyalty.avsc ..\samples\GBB.SchemaRegistry.IsolatedFunctions\
avrogen -s CustomerLoyalty.avsc ..\samples\GBB.SchemaRegistry.Producer\
avrogen -s cloudevents.avsc ..\samples\GBB.SchemaRegistry.Consumer\
avrogen -s cloudevents.avsc ..\samples\GBB.SchemaRegistry.Producer\

Write-Host '  POCOs generated.'