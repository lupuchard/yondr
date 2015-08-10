using System;
using System.Collections.Concurrent;

namespace Net {

/// A concurrent queue that replaces duplicates.
public class Queue<T> {
	public void Enqueue(int entity, T value) {
		Entry entry;
		if (map.TryGetValue(entity, out entry)) {
			entry.Value = value;
		} else {
			entry = new Entry();
			entry.Entity = entity;
			entry.Value  = value;
			queue.Enqueue(entry);
			map.TryAdd(entity, entry);
		}
	}

	public Entry Dequeue() {
		Entry entry;
		if (queue.TryDequeue(out entry)) {
			Entry cSharpIsDump;
			map.TryRemove(entry.Entity, out cSharpIsDump);
			return entry;
		}
		return null;
	}

	public class Entry {
		public int Entity { get; set; }
		public T Value { get; set;}
	}

	private readonly ConcurrentQueue<Entry> queue = new ConcurrentQueue<Entry>();
	private readonly ConcurrentDictionary<int, Entry> map = new ConcurrentDictionary<int, Entry>();
}

}
