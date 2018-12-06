function tryGetReadLock(record, readLockBinName, writeLockBinName)

	if(record[writeLockBinName] > 0) then
		return 0
	end
	
	local readLockVal = record[readLockBinName]
	readLockVal = readLockVal + 1
	record[readLockBinName] = readLockVal
	aerospike:update(record)
	return readLockVal

end

function tryGetWriteLock(record, readLockBinName, writeLockBinName)

	if(record[readLockBinName] > 0 or record[writeLockBinName] > 0) then
		return 0
	end
	
	local writeLockVal = record[writeLockBinName]
	writeLockVal = writeLockVal + 1
	record[writeLockBinName] = writeLockVal
	aerospike:update(record)
	return writeLockVal

end

function releaseReadLock(record, readLockBinName)

	local readLockVal = record[readLockBinName]
	readLockVal = readLockVal - 1
	record[readLockBinName] = readLockVal
	aerospike:update(record)
	return readLockVal

end

function releaseWriteLock(record, writeLockBinName)

	local writeLockVal = record[writeLockBinName]
	writeLockVal = writeLockVal - 1
	record[writeLockBinName] = writeLockVal
	aerospike:update(record)
	return writeLockVal

end