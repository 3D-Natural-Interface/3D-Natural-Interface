using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class MovieCtrl : MonoBehaviour
{

    public MovieTexture movie;
    public List<MovieTexture> movies;
    public int curIndex = 0;
    // Use this for initialization
    void OnEnable()
    {
        movie = movies[curIndex];
        GetComponent<RawImage>().texture = movie as MovieTexture;
        movie.Play();
    }
    void OnDisable()
    {
        movie.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (!movie.isPlaying)
        {
            movieChange();
        }
    }

    void movieChange()
    {
        movie.Stop();
        curIndex = ++curIndex % 13;
        movie = movies[curIndex];
        GetComponent<RawImage>().texture = movie as MovieTexture;
        movie.Play();
    }
    void moviePreviousChange()
    {
        movie.Stop();
        curIndex = curIndex - 1 < 0 ? 12 : --curIndex;
        movie = movies[curIndex];
        GetComponent<RawImage>().texture = movie as MovieTexture;
        movie.Play();
    }
}