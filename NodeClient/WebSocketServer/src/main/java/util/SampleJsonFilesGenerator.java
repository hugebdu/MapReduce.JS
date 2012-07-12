package util;

import com.google.common.base.Charsets;
import com.google.common.collect.ImmutableList;
import com.google.common.io.CharStreams;
import com.google.common.io.OutputSupplier;
import net.sf.json.JSONObject;

import java.io.*;
import java.util.List;

import static com.google.common.io.Files.newWriterSupplier;
import static java.lang.String.format;
import static org.apache.commons.lang.math.RandomUtils.nextInt;

/**
 * Created by IntelliJ IDEA.
 * User: daniels
 * Date: 7/12/12
 */
public class SampleJsonFilesGenerator
{
    private static final int MB = 1024 * 1024;
    private static final int[] sizes = { 1*MB, 3*MB, 5*MB, 10*MB };
    private static List<String> dictionary;

    public static void main(String[] args) throws IOException
    {
        dictionary = loadDictionary();

        for (int size : sizes)
        {
            generateJsonFileOfSize(size);
        }
    }

    private static void generateJsonFileOfSize(int size) throws IOException
    {
        StringWriter writer = new StringWriter();
        writer.write("[\n");
        boolean firstRecord = true;

        while (writer.getBuffer().length() < size)
        {
            String fileName = generateFileName();
            List<String> words = someRandomWords();

            if (!firstRecord)
                writer.write(",\n");

            writer.write('\t');

            new JSONObject()
                    .element("key", fileName)
                    .element("values", words).write(writer);

            firstRecord = false;
        }

        writer.write("\n]");

        OutputSupplier<OutputStreamWriter> supplier = newWriterSupplier(
                new File(getOutputDir(), format("sample_%dMB.json", (size / MB))),
                Charsets.UTF_8,
                false);

        CharStreams.write(writer.toString(), supplier);
    }

    private static String getOutputDir()
    {
        return System.getProperty("user.dir");
    }

    private static List<String> someRandomWords()
    {
        int numOfWords = 5 + nextInt(10);
        ImmutableList.Builder<String> builder = ImmutableList.builder();

        for (int i = 0; i < numOfWords; i++)
        {
            builder.add(randomWord());
        }

        return builder.build();
    }

    private static String generateFileName()
    {
        return randomWord() + ".txt";
    }

    private static String randomWord()
    {
        return dictionary.get(nextInt(dictionary.size()));
    }

    private static List<String> loadDictionary() throws IOException
    {
        return CharStreams.readLines(new InputStreamReader(SampleJsonFilesGenerator.class.getResourceAsStream("/dictionary.txt")));
    }
}
